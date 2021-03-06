﻿// <copyright file="MyHistoryNavigationItem.cs" company="Microsoft Corporation">Copyright Microsoft Corporation. All Rights Reserved. This code released under the terms of the Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.) This is sample code only, do not use in production environments.</copyright>
namespace Microsoft.ALMRangers.Samples.MyHistory
{
    using System;
    using System.ComponentModel.Composition;
    using System.Drawing;
    using Microsoft.TeamFoundation.Controls;
    using Microsoft.VisualStudio.Shell;

    [TeamExplorerNavigationItem(MyHistoryNavigationItem.LinkId, 200)]
    public class MyHistoryNavigationItem : TeamExplorerBaseNavigationItem
    {
        public const string LinkId = "e49a882b-1677-46a9-93b4-db290943bbcd";

        [ImportingConstructor]
        public MyHistoryNavigationItem([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.Text = "My History";
            this.IsVisible = true;

            Image bmp = Image.FromStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Microsoft.ALMRangers.Samples.MyHistory.Resources.MyHistory.png"));
            this.Image = bmp;
        }

        public override void Execute()
        {
            try
            {
                ITeamExplorer teamExplorer = GetService<ITeamExplorer>();
                if (teamExplorer != null)
                {
                   teamExplorer.NavigateToPage(new Guid(MyHistoryPage.PageId), null);
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex.Message, NotificationType.Error);
            }
        }
        
        public override void Invalidate()
        {
            base.Invalidate();
            this.IsVisible = true;
        }
    }
}