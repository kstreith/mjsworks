using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;

namespace Wyam.Core.Modules.Contents
{
    public class FilterAndSortPosts : IModule
    {
        const string publishedKey = "Published";
        private DateTime _currentDateTime { get; set; }
        public FilterAndSortPosts()
        {
            _currentDateTime = DateTime.Now;
        }

        public FilterAndSortPosts WithFilterDate(DateTime currentDateTime)
        {
            _currentDateTime = currentDateTime;
            return this;
        }

        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            IEnumerable<IDocument> outputs = inputs.Where(context, doc => {
                if (doc.ContainsKey(publishedKey))
                {
                    var published = doc.Get<DateTime>(publishedKey);
                    return published > _currentDateTime ? false : true;
                }
                return false;
            }).OrderByDescending(doc => doc.Get<DateTime>(publishedKey));

            return outputs;
        }
    }
}