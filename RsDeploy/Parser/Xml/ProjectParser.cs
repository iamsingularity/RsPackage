﻿using RsDeploy.Execution;
using RsDeploy.Parser.NamingConventions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RsDeploy.Parser.Xml
{
    public class ProjectParser
    {
        public string RootPath { get; private set; }
        public string ParentFolder { get; private set; }

        public INamingConvention NamingConvention { get; private set; }

        private FolderService folderService;

        private IEnumerable<IParser> ChildParsers { get; set; }
        public IDictionary<string, string> DataSources { get; } = new Dictionary<string, string>();

        public ProjectParser(IEnumerable<IParser> childParsers)
        {
            ChildParsers = childParsers;
        }

        public ProjectParser(ReportingService.ReportingService2010 rs, string parentFolder, string rootPath, INamingConvention namingConvention)
        {
            var childParsers = new List<IParser>();
            childParsers.Add(new DataSourceParser(new DataSourceService(rs)));
            folderService = new FolderService(rs);
            childParsers.Add(new FolderParser(folderService));
            childParsers.Add(new ReportParser(new ReportService(rs)));
            ChildParsers = childParsers;

            ParentFolder = parentFolder;
            RootPath = rootPath;
            NamingConvention = namingConvention;
        }

        public ProjectParser()
            : this(null, "/", string.Empty, new TitleToCamelCase())
        { }

        public void Execute(Stream stream)
        {
            var p = "/";
            foreach(var f in ParentFolder.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
            {
                folderService.Create(f, p);
                p += p == "/" ? f: "/" + f;
            }

            var xmlDoc = new XmlDocument();
            using (StreamReader reader = new StreamReader(stream))
                xmlDoc.Load(reader);

            var root = xmlDoc.FirstChild.NextSibling;
            foreach (var childParser in ChildParsers)
            {
                childParser.Root = this;
                childParser.Parent = null;
                childParser.ParentPath = ParentFolder;

                if (childParser is IParserPathable)
                    ((IParserPathable)childParser).RootPath = RootPath;

                childParser.Execute(root);
            }
                
        }
    }
}
