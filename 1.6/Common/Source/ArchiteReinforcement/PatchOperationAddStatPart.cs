using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using System.Xml;

namespace ArchiteReinforcement
{
    class PatchOperationAddStatPart : PatchOperationPathed
    {
#pragma warning disable CS0649
        private XmlContainer value;
        bool prepend = false;
#pragma warning restore CS0649

        protected override bool ApplyWorker(XmlDocument xml)
        {
            XmlNode node = value.node;
            foreach (object selectNode in xml.SelectNodes(this.xpath))
            {
                XmlNode xmlNode = selectNode as XmlNode;
                XmlNode element = xmlNode["parts"];
                if (element == null)
                {
                    element = xmlNode.OwnerDocument.CreateElement("parts");
                    xmlNode.AppendChild(element);
                }
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    if (prepend)
                        element.PrependChild(xmlNode.OwnerDocument.ImportNode(childNode, true));
                    else
                        element.AppendChild(xmlNode.OwnerDocument.ImportNode(childNode, true));
                }
                    
            }
            return true;
        }
    }
}
