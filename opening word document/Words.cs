﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opening_word_document
{
    class Words
    {
        public string Word { get; set; }
        public int DocFrequency { get; set; }
       public int Pageno { get; set; }
        //public Dictionary<int,int> PagenoWithFrequency { get; set; }
        public int CorpusFrequency { get; set; }
        //public int[] SentencesOccuredIn { get; set; }
        public Dictionary<int, int> SentencenoWithFrequency { get; set; }
        
    }
}
