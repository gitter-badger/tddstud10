﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using R4nd0mApps.TddStud10.Common.Domain;

namespace R4nd0mApps.TddStud10.TestHost
{
    [Serializable]
    public class PerAssemblyTestIds : SerializableDictionary<FilePath, List<TestId>>
    {
        public PerAssemblyTestIds()
        {
        }

        public PerAssemblyTestIds(IEnumerable<KeyValuePair<FilePath, List<TestId>>> collection)
            : base(collection)
        {
        }

        public static PerAssemblyTestIds Deserialize(string file)
        {
            return Deserialize<PerAssemblyTestIds>(file);
        }
    }
}