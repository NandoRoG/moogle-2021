using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoogleEngine
{
    public class Stemm
    {
        //Determina si el caracter es una vocal.
        private bool Vowel(char c)
        {
            return (c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u'
                    || c == 'á' || c == 'é' || c == 'í' || c == 'ó' || c == 'ú');
        }
        
        //Retorna la posicion de la primera vocal encontrada
        private int NextVowelPos(string word, int start)
        {
            for (int i = start; i < word.Length; i++)
            {
                if (Vowel(word[i]))return i;
            }
            return word.Length;
        }
        
        //Retorna la posicion de la primera consonante encontrada
        private int NextConsonantPos(string word, int start)
        {
            for (int i = start; i < word.Length; i++)
            {
                if (!Vowel(word[i]))return i;
            }
            return word.Length;
        }
        
        //Determina si el sufijo esta presente en una palabra
        private bool Endsin(string word, string suffix)
        {
            if (word.Length < suffix.Length)return false;
            
            return MySubStr(word, -suffix.Length).Equals(suffix);
        }

        // Retorna el sufijo mas largo presente en una palabra
        private string LongestSuff(string word, string[] suffixes)
        {
            string tmp = "";
            foreach (string suff in suffixes)
            {
                if (Endsin(word, suff))
                {
                    if (suff.Length >= tmp.Length)tmp = suff;
                }
            }
            return tmp;
        }

        // Retorna una palabra sin acentos
        private string RemoveAccent(string word)
        {
            word = word.Replace('á', 'a');
            word = word.Replace('é', 'e');
            word = word.Replace('í', 'i');
            word = word.Replace('ó', 'o');
            word = word.Replace('ú', 'u');
            return word;
        }

        
        //Retorna una palabra con sus sufijos eliminados
        public string Stemming(string word)
        {
            int len = word.Length;
            //si es chiquita la palabra solo se le quitan las tildes 
            if (len <= 2)
            {
                return RemoveAccent(word);
            }


            int r1, r2, rv;
            r1 = r2 = rv = len;
            //r1 es la region despues de la primera consonante q es precedida por una vocal o es nula si no existe tal
            for (int i = 0; i < (len - 1); i++)
            {
                if (Vowel(word[i]) && !Vowel(word[i + 1]))
                {
                    r1 = i + 2;
                    break;
                }
            }
            //r2 es la region despues de la primera consonante precedida por una vocal que esta en r1 
            for (int i = r1; i < (len - 1); i++)
            {
                if (Vowel(word[i]) && !Vowel(word[i + 1]))
                {
                    r2 = i + 2;
                    break;
                }
            }

            if (len > 3)
            {
                //si la segunda letra es consonante
                if (!Vowel(word[1]))
                {
                    //rv es la region despues de la proxima vocal
                    
                    rv = NextVowelPos(word, 2) + 1;
                }
                //si las dos primeras letras son vocales
                else if (Vowel(word[0]) && Vowel(word[1]))
                {
                    //rv es la region despues de la proxima consonante
                   
                    rv = NextConsonantPos(word, 2) + 1;
                }
                else
                {
                    //cuando es consonante y vocal rv = 3
                    rv = 3;
                }
            }

            string r1_txt = MySubStr(word, r1);
            string r2_txt = MySubStr(word, r2);
            string rv_txt = MySubStr(word, rv);
            //guarda la palabra origen
            string word_orig = word;
            //primer paso quitar los pronombres
            
            string[] pronoun_suf = {"me", "se", "sela", "selo", "selas",
                    "selos", "la", "le", "lo", "las", "les", "los", "nos" };
            string[] pronoun_suf_pre1 = { "iéndo", "ándo", "ár", "ér", "ír" };
            string[] pronoun_suf_pre2 = { "ando", "iendo", "ar", "er", "ir" };
            
            string suf = LongestSuff(word, pronoun_suf);
            //si existe tal sufijo
            if (!suf.Equals(""))
            {
                //verificar la existencia de los sufijos de la segunda lista
                string pre_suff = LongestSuff(MySubStr2(rv_txt, 0, -suf.Length),
                        pronoun_suf_pre1);
                //si existe
                if (!pre_suff.Equals(""))
                {
                    //remuevo el acento
                    word = RemoveAccent(MySubStr2(word, 0, -suf.Length));
                }
                else
                {
                    pre_suff = LongestSuff(MySubStr2(rv_txt, 0, -suf.Length),
                            pronoun_suf_pre2);
                    if (!pre_suff.Equals("")
                            || (Endsin(word, "yendo") && (MySubStr2(word,
                            -suf.Length - 6, 1).Equals("u"))))
                    {
                        word = MySubStr2(word, 0, -suf.Length);
                    }
                }
            }

            if (!word.Equals(word_orig))
            {
                r1_txt = MySubStr(word, r1);
                r2_txt = MySubStr(word, r2);
                rv_txt = MySubStr(word, rv);
            }
            string word_after0 = word;

            if (!(suf = LongestSuff(r2_txt, new string[] {"anza", "anzas", "ico", "ica",
                    "icos", "icas", "ismo", "ismos", "able", "ables", "ible",
                    "ibles", "ista", "istas", "oso", "osa", "osos", "osas",
                    "amiento", "amientos", "imiento", "imientos"})).Equals(""))
            {
                word = MySubStr2(word, 0, -suf.Length);
            }
            else if (!(suf = LongestSuff(r2_txt, new string[] {"icadora", "icador",
                  "icación", "icadoras", "icadores", "icaciones", "icante",
                  "icantes", "icancia", "icancias", "adora", "ador", "ación",
                  "adoras", "adores", "aciones", "ante", "antes", "ancia", "ancias" })).Equals(""))
            {
                word = MySubStr2(word, 0, -suf.Length);
            }
            else if (!(suf = LongestSuff(r2_txt, new string[] { "logía", "logías" })).Equals(""))
            {
                word = MySubStr2(word, 0, -suf.Length) + "log";
            }
            else if (!(suf = LongestSuff(r2_txt, new string[] { "ución", "uciones"})).Equals(""))
            {
                word = MySubStr2(word, 0, -suf.Length) + "u";
            }
            else if (!(suf = LongestSuff(r2_txt, new string[] { "encia", "encias" })).Equals(""))
            {
                word = MySubStr2(word, 0, -suf.Length);
            }
            else if (!(suf = LongestSuff(r2_txt, new string[] { "ativamente", "ivamente",
                  "osamente", "icamente", "adamente", "izacion"  })).Equals(""))
            {
                word = MySubStr2(word, 0, -suf.Length);
            }
            else if (!(suf = LongestSuff(r1_txt, new string[] { "amente" })).Equals(""))
            {
                word = MySubStr2(word, 0, -suf.Length);
            }
            else if (!(suf = LongestSuff(r1_txt, new string[] { "ita","ito", "ucha","erio" })).Equals(""))
            {
                word = MySubStr2(word, 0, -suf.Length);
            }
            else if (!(suf = LongestSuff(r2_txt, new string[] { "antemente", "ablemente",
                  "iblemente", "mente" })).Equals(""))
            {
                word = MySubStr2(word, 0, -suf.Length);
            }
            else if (!(suf = LongestSuff(r2_txt, new string[] { "abilidad", "abilidades",
                  "icidad", "icidades", "ividad", "ividades", "idad", "idades" })).Equals(""))
            {
                word = MySubStr2(word, 0, -suf.Length);
            }
            else if (!(suf = LongestSuff(r2_txt, new string[] { "ativa", "ativo", "ativas",
                  "ativos", "iva", "ivo", "ivas", "ivos" })).Equals(""))
            {
                word = MySubStr2(word, 0, -suf.Length);
            }

            if (!word.Equals(word_after0))
            {
                r1_txt = MySubStr(word, r1);
                r2_txt = MySubStr(word, r2);
                rv_txt = MySubStr(word, rv);
            }
            string word_after1 = word;

            if (word_after0.Equals(word_after1))
            {
                // Hace el paso 2a si no se removio nada en el paso uno 
                if ((!(suf = LongestSuff(rv_txt,
                        new string[] { "ya", "ye", "yan", "yen", "yeron", "yendo", "yo",
                        "yó", "yas", "yes", "yais", "yamos" })).Equals(""))
                        && (MySubStr2(word, -suf.Length - 1, 1).Equals("u")))
                {
                    word = MySubStr2(word, 0, -suf.Length);
                }

                if (!word.Equals(word_after1))
                {
                    r1_txt = MySubStr(word, r1);
                    r2_txt = MySubStr(word, r2);
                    rv_txt = MySubStr(word, rv);
                }
                string word_after2a = word;
                // Hace el paso 2b si el 2a fue hecho y fallo en remover un sufijo
               
                if (word_after2a.Equals(word_after1))
                {
                    if (!(suf = LongestSuff(rv_txt, new string[] { "arían", "arías",
                            "arán", "arás", "aríais", "aría", "aréis", "aríamos",
                            "aremos", "ará", "aré", "erían", "erías", "erán",
                            "erás", "eríais", "ería", "eréis", "eríamos", "eremos",
                            "erá", "eré", "irían", "irías", "irán", "irás",
                            "iríais", "iría", "iréis", "iríamos", "iremos", "irá",
                            "iré", "aba", "ada", "ida", "ía", "ara", "iera", "ad",
                            "ed", "id", "ase", "iese", "aste", "iste", "an",
                            "aban", "ían", "aran", "ieran", "asen", "iesen",
                            "aron", "ieron", "ado", "ido", "ando", "iendo", "ió",
                            "ar", "er", "ir", "as", "abas", "adas", "idas", "ías",
                            "aras", "ieras", "ases", "ieses", "ís", "áis", "abais",
                            "íais", "arais", "ierais", "aseis", "ieseis", "asteis",
                            "isteis", "ados", "idos", "amos", "ábamos", "íamos",
                            "imos", "áramos", "iéramos", "iésemos", "ásemos" })).Equals(""))
                    {
                        word = MySubStr2(word, 0, - suf.Length);
                    }
                    else if (!(suf = LongestSuff(rv_txt, new string[] { "en", "es", "éis", "emos" })).Equals(""))
                    {
                        word = MySubStr2(word, 0, - suf.Length);
                        if (Endsin(word, "gu"))
                        {
                            word = MySubStr2(word, 0, -1);
                        }
                    }
                }
            }

            // Siempre hace el paso tres
            r1_txt = MySubStr(word, r1);
            r2_txt = MySubStr(word, r2);
            rv_txt = MySubStr(word, rv);

            if (!(suf = LongestSuff(rv_txt, new string[] { "os", "a", "o", "á", "í", "ó" })).Equals(""))
            {
                word = MySubStr2(word, 0, - suf.Length);
            }
            else if (!(suf = LongestSuff(rv_txt, new string[] { "e", "é" })).Equals(""))
            {
                word = MySubStr2(word, 0, -1);
                rv_txt = MySubStr(word, rv);
                if (Endsin(rv_txt, "u") && Endsin(word, "gu"))
                {
                    word = MySubStr2(word, 0, -1);
                }
            }

            return RemoveAccent(word);
        }

        
        //Retorna una subcadena de un String
        private string MySubStr2(string word, int beginIndex, int length)
        {
            if (beginIndex == length)return "";

            else
            {
                if (beginIndex >= 0)
                { // incio positivo
                    int endIndex;
                    if (length >= 0)
                    { // longitud positiva
                        endIndex = beginIndex + length;
                        if (endIndex > word.Length)
                        {
                            word = word.Substring(beginIndex, word.Length - beginIndex);
                            return word;
                        }
                        else
                        {
                            word = word.Substring(beginIndex, endIndex  - beginIndex);
                            return word;
                        }
                    }
                    else
                    { // longitud negativa
                        endIndex = word.Length + length;
                        try
                        {
                            word = word.Substring(beginIndex, endIndex - beginIndex);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            word = "";
                        }
                        return word;
                    }

                }
                else
                {// incio negativo
                    int endIndex;
                    int newBeginIndex;
                    if ((length >= 0))
                    { // longitud positiva
                        newBeginIndex = word.Length + beginIndex;
                        endIndex = newBeginIndex + length;
                        if (endIndex > word.Length)
                        {
                            word = word.Substring(newBeginIndex, word.Length - newBeginIndex);
                            return word;
                        }
                        else
                        {
                            word = word.Substring(newBeginIndex, endIndex - newBeginIndex);
                            return word;
                        }
                    }
                    else
                    { // longitud negativa
                        newBeginIndex = word.Length + beginIndex;
                        endIndex = word.Length + length;

                        try
                        {
                            word = word.Substring(newBeginIndex, endIndex - newBeginIndex);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            word = "";
                        }

                        return word;
                    }
                }

            }

        }

        private string MySubStr(string word, int beginIndex)
        {
            //si es mayor el comienzo a la palabra
            if (Math.Abs(beginIndex) > word.Length)
            {
                //devuleve la palabra
                return word;
            }
            //si el comienzo es mayor que cero
            else if (beginIndex >= 0)
            {
                //devuelve el substring comenzando por la posicion
                return word.Substring(beginIndex, word.Length - beginIndex);
            }
            else
            {
                //si el comienzo es negativo el substring comienza en el lenght - el comienzo
                return word.Substring(word.Length + beginIndex, word.Length - (word.Length + beginIndex));
            }
        }

       

      

    }
}



