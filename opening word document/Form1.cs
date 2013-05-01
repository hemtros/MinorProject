using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Word;
using iTextSharp.text.pdf.parser;
using Application = System.Windows.Forms.Application;
using System.Runtime.InteropServices;
//using iTextSharp;
using iTextSharp.text.pdf;
using System.Collections;







namespace opening_word_document
{
    public partial class MainForm : Form
    {
        private OpenFileDialog ofd;
        private string pathToPdf;
        private int _tUniqueWordsinPage;
        private Words[] wordarray;
        private List<Words> wordList; 
        private List<string> nrSplittedWords;
        private List<string> splittedWords;
        private List<int> countList;
        private List<Sentences> sentenceList;
        private Sentences[] sentencearray;
        private int currentcountofsentences;
        private List<UniqueWords> UniqueWordList;
        private UniqueWords[] uniquewordarray;
        private List<string> UniqueWordsinCorpus;
 

        public MainForm()
        {
            InitializeComponent();
            ofd = new OpenFileDialog();
            _tUniqueWordsinPage = 0;
            splittedWords=new List<string>();
            countList=new List<int>();
            wordList=new List<Words>();
            nrSplittedWords=new List<string>();
            sentenceList=new List<Sentences>();
            currentcountofsentences = 0;
            UniqueWordList=new List<UniqueWords>();
           
            UniqueWordsinCorpus=new List<string>();

        }

        
        
        private void ConvertButton_Click(object sender, EventArgs e)
        {
           
            
            
            //POS Tagging code
            POSTagged post = new POSTagged();

            POSTagger.mModelPath = "Models\\";

            

            string content = DocText.Text;

            string[] tokenize = POSTagger.TokenizeSentence(content);
            string[] POS = POSTagger.PosTagTokens(tokenize);
            string POSTextbox = string.Empty;
            for (int i = 0; i < POS.Length; i++)
            {
                //NN Noun, singular or mass
                //NNS Noun, plural
                //NNP Proper noun, singular
                //NNPS Proper noun, plural
                if (POS[i] == "NN" || POS[i] == "NNS" || POS[i] == "NNP" || POS[i] == "NNPS")
                    POSTextbox = POSTextbox + (tokenize[i] + "/" + POS[i] + "  ");


            }
            post.PosTaggedText = POSTextbox;

            this.Hide();

            post.ShowDialog();
          
            this.Show();
            
        }

        private void BrowseBtn_Click(object sender, EventArgs e)
        {
            
            ofd.Filter = "PDF(*.pdf) | *.pdf|Word Document(*.doc)|*.doc|Open Doc Text(*.odt)|*.odt|Microsoft XPS(*.xps)|*.xps";
            
            
            if (ofd.ShowDialog() == DialogResult.OK)
            textPathName.Text = ofd.FileName;
            
            
        }

        private void ReadButton_Click(object sender, EventArgs e)
        {
            if (textPathName.Text.Length > 0)
            {
                ReadFileContent(textPathName.Text);
            }
            else
            {
                MessageBox.Show("Enter a valid file path");
            }
        }

      
        public void ReadFileContent(string path)
        {
            string ext = Path.GetExtension(path);
            if (ext == ".doc")
            {

                try
                {
                   

                    Word2pdf w2p=new Word2pdf();
                    pathToPdf= w2p.ConvertToPdf(path);
                    ReadPdf(pathToPdf);

                }

                catch (COMException)
                {
                    MessageBox.Show("Unable to read this document.  It may be corrupt.");

                }
            }

            else
            {
                ReadPdf(path);
            }

        }

        public void ReadPdf(string path)
        {
            try
            {
               
                PdfReader pdfr = new PdfReader(path);
                StringBuilder pdfText = new StringBuilder();
                
              
                //loop to read pdf page by page
               
                for (int page = 1; page <= pdfr.NumberOfPages; page++)
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    string currentText = PdfTextExtractor.GetTextFromPage(pdfr, page, strategy);
                 
                    

                    currentText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));

                  

                   
                    POSTagger.mModelPath = "Models\\";
                    string[] tSplittedWords = GetWords(currentText);

                    string[] sentences = POSTagger.SplitSentences(currentText);

                    sentencearray=new Sentences[sentences.Count()];

                    for (int i = 0; i < sentences.Count(); i++)
                    {
                        sentencearray[i]=new Sentences();
                        
                        sentencearray[i].SentenceNumber = i+1+currentcountofsentences;
                        
                        sentencearray[i].SentenceString = sentences[i];
                        sentenceList.Add(sentencearray[i]);
                    }

                    currentcountofsentences += sentences.Count();

                    string[] POS = POSTagger.PosTagTokens(tSplittedWords);

                    splittedWords.Clear();
                    nrSplittedWords.Clear();
                    countList.Clear();
                    for (int i = 0; i < POS.Length; i++)
                    {
                        //NN Noun, singular or mass
                        //NNS Noun, plural
                        //NNP Proper noun, singular
                        //NNPS Proper noun, plural
                        if (POS[i] == "NN" || POS[i] == "NNS" || POS[i] == "NNP" || POS[i] == "NNPS")

                            splittedWords.Add(tSplittedWords[i]); 


                    }





                    nrSplittedWords = splittedWords.Distinct().ToList();
                    
                    _tUniqueWordsinPage = nrSplittedWords.Count();
                  
                   //calculating frequency of words in each page
                    for (int i = 0; i < nrSplittedWords.Count();i++ )
                    {
                        string searchItem = nrSplittedWords[i];

                        
                        int count=0;
                        for (int j = 0; j < splittedWords.Count();j++ )
                        {
                            if (searchItem == splittedWords[j])
                                count++;
                        }
                           
                            countList.Add(count);

                    }
                    wordarray=new Words[nrSplittedWords.Count()];
                   
                    for (int i = 0; i < nrSplittedWords.Count(); i++)
                    {
                        wordarray[i] = new Words();

                       
                        wordarray[i].Word = nrSplittedWords[i];
                        wordarray[i].DocFrequency = countList[i];
                        wordarray[i].Pageno=page;
                      
                        wordList.Add(wordarray[i]);
                        
                    }

                    foreach (string s in nrSplittedWords)
                    {
                        UniqueWordsinCorpus.Add(s);
                    }

                                      
                        pdfText.Append(currentText);
                    
                }                  //end of page loop

                pdfr.Close();


                UniqueWordsinCorpus = UniqueWordsinCorpus.Distinct().ToList();
                UniqueWordsinCorpus.Sort();
                


                foreach (Words w in wordList)
                {
                    int corf = 0;
                    foreach (Words w1 in wordList)
                    {
                        if (w.Word == w1.Word)
                            corf = corf + w1.DocFrequency;
                     

                    }

                    w.CorpusFrequency = corf;
                }
                
                
                foreach(Words w in wordList)
                {
                    w.SentencenoWithFrequency=new Dictionary<int, int>();
                    foreach(Sentences s in sentenceList)
                    {
                        int sentfreq = 0;
                        string[] splittedwordsofsentence = GetWords(s.SentenceString);
                        for(int i=0;i<splittedwordsofsentence.Count();i++)
                        {
                            if (w.Word == splittedwordsofsentence[i])
                                sentfreq++;
                        }
                       w.SentencenoWithFrequency.Add(s.SentenceNumber,sentfreq);
                    }
                }

                

                //wordList.Sort(delegate(Words w1, Words w2) { return w1.Word.CompareTo(w2.Word); });

                wordList.Sort((w1,w2) => w1.Word.CompareTo(w2.Word));

                
                //copying words from wordList of Words to uniquewordlist of uniquewords while removing the redundant entry

                uniquewordarray=new UniqueWords[UniqueWordsinCorpus.Count];
                for (int i = 0; i < UniqueWordsinCorpus.Count; i++)
                {
                   

                    uniquewordarray[i] = new UniqueWords();
                    uniquewordarray[i].SentencenoWithFrequency = new Dictionary<int, int>();
                    uniquewordarray[i].PagenoWithFrequency = new Dictionary<int, int>();
                    foreach (Words w in wordList)
                    {
                        if (UniqueWordsinCorpus[i] == w.Word)
                        {
                            if(uniquewordarray[i].Term==null)
                            uniquewordarray[i].Term = w.Word;

                            
                            uniquewordarray[i].CorpusFrequency = w.CorpusFrequency;

                            uniquewordarray[i].SentencenoWithFrequency = w.SentencenoWithFrequency;
                           
                            uniquewordarray[i].PagenoWithFrequency.Add(w.Pageno,w.DocFrequency);
                            
                        }
                    }

                    UniqueWordList.Add(uniquewordarray[i]);
                   
                }


                //Displaying uniquewords with their attribute values

                foreach (UniqueWords uw in UniqueWordList)
                {
                    DocText.AppendText(uw.Term + "........");
                    DocText.AppendText(uw.CorpusFrequency.ToString() + "\n");
                    DocText.AppendText("Sentence no with frequency \n");
                    List<KeyValuePair<int, int>> list = uw.SentencenoWithFrequency.ToList();
                    foreach (KeyValuePair<int, int> pair in list)
                    {
                        if (pair.Value > 0)
                        {
                            DocText.AppendText(pair.Key.ToString() + ".......");
                            DocText.AppendText(pair.Value.ToString() + Environment.NewLine);
                        }

                    }
                    DocText.AppendText("Page no with freqency \n");
                    List<KeyValuePair<int, int>> list1 = uw.PagenoWithFrequency.ToList();
                    foreach (KeyValuePair<int, int> pair in list1)
                    {
                        if (pair.Value > 0)
                        {
                            DocText.AppendText(pair.Key.ToString() + ".......");
                            DocText.AppendText(pair.Value.ToString() + Environment.NewLine);
                        }

                    }
                }

                    foreach (Words w in wordList)
                    {
                        DocText.AppendText(w.Word + Environment.NewLine);
                        List<KeyValuePair<int, int>> list = w.SentencenoWithFrequency.ToList();
                        foreach (KeyValuePair<int, int> pair in list)
                        {
                            if (pair.Value > 0)
                            {
                                DocText.AppendText(pair.Key.ToString() + ".......");
                                DocText.AppendText(pair.Value.ToString() + Environment.NewLine);
                            }

                        }

                    }
                
                //Diplaying words with their page no and Docfrequency using objects
                foreach (Words w in wordList)
                {
                    WordnFrequencyTxtBox.AppendText(w.Pageno + "---------" + w.Word + "  -----------" + w.DocFrequency +"-----------"+w.CorpusFrequency+ System.Environment.NewLine);
                }


                
            }
            catch (Exception se)
            {

                MessageBox.Show(se.Message);
            }

        }

         static string[] GetWords(string input)
        {
            MatchCollection matches = Regex.Matches(input, @"\b[\w']*\b");

            var words = from m in matches.Cast<Match>()
                        where !string.IsNullOrEmpty(m.Value)
                        select TrimSuffix(m.Value);

            return words.ToArray();
        }

        static string TrimSuffix(string word)
        {
            int apostrapheLocation = word.IndexOf('\'');
            if (apostrapheLocation != -1)
            {
                word = word.Substring(0, apostrapheLocation);
            }

            return word;
        }


  
        
        
    }
}
