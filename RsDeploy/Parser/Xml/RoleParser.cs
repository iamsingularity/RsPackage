﻿using RsDeploy.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RsDeploy.Parser.Xml
{
    class RoleParser : IParser
    {
        private RoleService roleService;

        private IEnumerable<IParser> ChildrenParsers;

        public RoleParser()
        {
            ChildrenParsers = new List<IParser>();
        }

        public void Execute(XmlNode node, string parent)
        {
            var roleNodes = node.SelectNodes("Role");
            foreach (XmlNode roleNode in roleNodes)
            {
                var name = roleNode.Attributes["Name"].Value;
                var description = roleNode.SelectSingleNode("Description")?.Value;
                var tasks = new List<string>();
                var taskNodes = roleNode.SelectNodes("Task");
                foreach (XmlNode taskNode in taskNodes)
                    tasks.Add(taskNode.Value);

                roleService.Create(name, description, tasks.ToArray());
            }
        }
    }
}