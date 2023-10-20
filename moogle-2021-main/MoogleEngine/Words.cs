using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoogleEngine
{
    public class Words
    {
        static StreamReader reader = new StreamReader("sinonimos.txt");
         
        static HashSet<string>[] words = new HashSet<string>[3557];
        public static Dictionary<string, HashSet<string>> synonyms = new Dictionary<string, HashSet<string>>();
        //carga los conjuntos de palabras sinonimas 
        public static void LoadWords()
        {
            for(int i = 0; i < words.Length; i++)
            {
                words[i] = Moogle.Divide(reader.ReadLine()!).ToHashSet(); 
               
            }
        }

        public static void LoadSynonyms()
        {
            for(int i =0; i < words.Length; i++)
            {
                //por cada palabra sinonima 
                foreach(string word in words[i])
                {
                    //creo el hashSet de sinonimos removiendo la palabra
                    HashSet<string> Syn = new HashSet<string>();
                    foreach(string word2 in words[i])
                    {
                        Syn.Add(word2);
                    }
                    Syn.Remove(word);
                    //verifica si mi diccionario tiene o no la palabra
                    if (!synonyms.ContainsKey(word))
                    {
                        synonyms.Add(word, Syn);
                        
                    }
                    else synonyms[word].UnionWith(Syn);
                }
            }
        }
        
        
        //metodo para contar las lineas de un txt 
        public static int CountLines()
        {
            int count = 0;
            for(int i = 0; ; i++)
            {
                if (reader.ReadLine() != "")
                {
                    count++;
                }
                else break;
            }
            return count;
        }
    }

}
