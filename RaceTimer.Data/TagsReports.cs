using System;

namespace RaceTimer.Data
{
    public class TagsReports : EventArgs
    {
        private readonly string _tagsReportsText;

        public TagsReports(string tagsReportsText)
        {
            this._tagsReportsText = tagsReportsText;
        }

        public string TagsReportsText()
        {
            return _tagsReportsText;
        }
    }
}
