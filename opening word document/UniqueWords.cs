using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opening_word_document
{
    class UniqueWords
    {
        public string Term { get; set; }
        public Dictionary<int,int> PagenoWithFrequency { get; set; }
        public int CorpusFrequency { get; set; }
        public Dictionary<int,int> SentencenoWithFrequency { get; set; }
    }
}
