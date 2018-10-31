using System.Collections.Generic;
using System.IO;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;

namespace MjsWorks.Gallery.Wyam
{
    public class SetNavigation : IModule
    {
        private string _tabName;
        public SetNavigation ToTab(string tabName)
        {
            _tabName = tabName;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.Select(context, doc =>
            {
                var tabName = doc.String("NavigationTab") ?? _tabName;
                return context.GetDocument(doc, new Dictionary<string, object>
                {
                    ["NavigationTab"] = tabName
                });
            });
        }
    }
}
