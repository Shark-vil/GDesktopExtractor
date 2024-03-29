﻿using CSharpGmaReaderLibrary.Models;
using System;

namespace GarrysmodDesktopAddonExtractor.Models
{
    public struct AddonDataRowModel
    {
        public AddonInfoModel AddonInfo;
        public bool RowVisible;

		public long? AddonId
		{
			get { return AddonInfo.AddonId; }
			set { AddonInfo.AddonId = value; }
		}

		public string? AddonName
        {
            get { return AddonInfo.Name; }
            set { AddonInfo.Name = value; }
        }

        public string? SourcePath
        {
            get { return AddonInfo.SourcePath; }
            set { AddonInfo.SourcePath = value; }
        }

        public DateTime AddonTimestamp
        {
            get { return AddonInfo.Timestamp; }
            set { AddonInfo.Timestamp = value; }
        }

        public AddonDataRowModel(AddonInfoModel addonInfo)
        {
            AddonInfo = addonInfo;
            RowVisible = true;
        }
    }
}
