﻿// <copyright file="ShelvesetsFullSection.cs" company="Microsoft Corporation">Copyright Microsoft Corporation. All Rights Reserved. This code released under the terms of the Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.) This is sample code only, do not use in production environments.</copyright>
namespace Microsoft.ALMRangers.Samples.MyHistory
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using EnvDTE80;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.Controls;
    using Microsoft.TeamFoundation.VersionControl.Client;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TeamFoundation.VersionControl;

    [TeamExplorerSection(ShelvesetsFullSection.SectionId, ShelvesetsPage.PageId, 10)]
    public class ShelvesetsFullSection : TeamExplorerBaseSection
    {
        public const string SectionId = "7E8B1F70-321D-4327-A744-7C14AE73B59F";
        private ObservableCollection<Shelveset> shelvesets = new ObservableCollection<Shelveset>();

        public ShelvesetsFullSection()
        {
            this.Title = "Shelvesets";
            this.IsVisible = true;
            this.IsExpanded = true;
            this.IsBusy = false;
            this.SectionContent = new ShelvesetsFullSectionView();
            this.View.ParentSection = this;
        }

        public ObservableCollection<Shelveset> Shelvesets
        {
            get
            {
                return this.shelvesets;
            }

            protected set
            {
                this.shelvesets = value;
                this.RaisePropertyChanged("Shelvesets");
            }
        }

        protected ShelvesetsFullSectionView View
        {
            get { return this.SectionContent as ShelvesetsFullSectionView; }
        }

        public static VersionControlExt GetVersionControlExt(IServiceProvider serviceProvider)
        {
            if (serviceProvider != null)
            {
                DTE2 dte = serviceProvider.GetService(typeof(SDTE)) as DTE2;
                if (dte != null)
                {
                    return dte.GetObject("Microsoft.VisualStudio.TeamFoundation.VersionControl.VersionControlExt") as VersionControlExt;
                }
            }

            return null;
        }

        public void ViewShelvesetDetails(Shelveset shelveset)
        {
            ITeamExplorer teamExplorer = this.GetService<ITeamExplorer>();
            if (teamExplorer != null)
            {
                teamExplorer.NavigateToPage(new Guid(TeamExplorerPageIds.ShelvesetDetails), shelveset);
            }
        }

        public async override void Initialize(object sender, SectionInitializeEventArgs e)
        {
            base.Initialize(sender, e);

            // If the user navigated back to this page, there could be saved context information that is passed in
            var sectionContext = e.Context as ChangesSectionContext;
            if (sectionContext != null)
            {
                // Restore the context instead of refreshing
                ChangesSectionContext context = sectionContext;
                this.Shelvesets = context.Shelvesets;
            }
            else
            {
                // Kick off the refresh
                await this.RefreshAsync();
            }
        }

        /// <summary>
        /// Refresh override.
        /// </summary>
        public async override void Refresh()
        {
            base.Refresh();
            await this.RefreshAsync();
        }

        /// <summary>
        /// Save contextual information about the current section state.
        /// </summary>
        public override void SaveContext(object sender, SectionSaveContextEventArgs e)
        {
            base.SaveContext(sender, e);

            // Save our current so when the user navigates back to the page the content is restored rather than requeried
            ChangesSectionContext context = new ChangesSectionContext { Shelvesets = this.Shelvesets };
            e.Context = context;
        }

        /// <summary>
        /// ContextChanged override.
        /// </summary>
        protected override async void ContextChanged(object sender, TeamFoundation.Client.ContextChangedEventArgs e)
        {
            base.ContextChanged(sender, e);

            // If the team project collection or team project changed, refresh the data for this section
            if (e.TeamProjectCollectionChanged || e.TeamProjectChanged)
            {
                await this.RefreshAsync();
            }
        }

        /// <summary>
        /// Get the parameters for the shelveset query.
        /// </summary>
        private void GetShelvesetParameters(VersionControlServer vcs, out string user)
        {
            user = vcs.AuthorizedUser;
        }

        private async Task RefreshAsync()
        {
            try
            {
                // Set our busy flag and clear the previous data
                this.IsBusy = true;
                this.Shelvesets.Clear();

                ObservableCollection<Shelveset> lshelvesests = new ObservableCollection<Shelveset>();

                // Make the server call asynchronously to avoid blocking the UI
                await Task.Run(() =>
                {
                    ITeamFoundationContext context = this.CurrentContext;
                    if (context != null && context.HasCollection && context.HasTeamProject)
                    {
                        VersionControlServer vcs = context.TeamProjectCollection.GetService<VersionControlServer>();
                        if (vcs != null)
                        {
                            // Ask the derived section for the history parameters
                            string user;
                            GetShelvesetParameters(vcs, out user);
                            foreach (Shelveset shelveset in vcs.QueryShelvesets(null, user))
                            {
                                lshelvesests.Add(shelveset);
                            }
                        }
                    }
                });

                // The shelvesets are not in the right order, lets sort them by date
                var sortedShelvesets = (
                    from ss in lshelvesests
                    orderby ss.CreationDate descending
                    select ss).ToList();

                ObservableCollection<Shelveset> lshelvesests2 = new ObservableCollection<Shelveset>();
                foreach (var s in sortedShelvesets)
                {
                    lshelvesests2.Add(s);
                }

                this.Shelvesets = lshelvesests2;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex.Message, NotificationType.Error);
            }
            finally
            {
                // Always clear our busy flag when done
                this.IsBusy = false;
            }
        }
    }
}
