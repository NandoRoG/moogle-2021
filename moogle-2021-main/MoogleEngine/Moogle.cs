namespace MoogleEngine;


public static class Moogle
{
    #region Variables 
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    static List<string> allfiles;
    static string[] content;
    static string[] titles;
    static Dictionary<string, float>[] queryRelevance;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    static Dictionary<string, List <int>> vocabulary = new Dictionary<string, List <int>>();
    static Dictionary<int,Dictionary<string,HashSet<int>>> wordsInd = new Dictionary<int,Dictionary<string, HashSet<int>>>();
    static Dictionary<string,string> modifications = new Dictionary<string,string>();
    static Dictionary<string,HashSet<string>> roots = new Dictionary<string,HashSet<string>>();
    static int count = 0;
    static char[] alphabet = { 'a', 'á', 'b', 'c', 'd', 'e', 'é', 'f', 'g', 'h', 'i', 'í', 'j', 'k', 'l', 'm', 'n', 'ñ', 'o', 'ó', 'p', 'q', 'r', 's', 't', 'u', 'ú', 'v', 'w', 'x', 'y', 'z' };
    public static bool AreadyInit = false;
    #endregion
    #region Inicio
    //metodo para inicializar
    public static void Initialize()
    {
        //Leer los archivos .txt
        allfiles = Directory.GetFiles(Directory.GetParent(System.Environment.CurrentDirectory)!.ToString() + @"\Content", "*.txt", SearchOption.AllDirectories).ToList();
        content = Content(allfiles);
        titles = Titles(allfiles);
        
        wordsInd = WordsInd();
        vocabulary = Vocabulary();
        Words.LoadWords();
        Words.LoadSynonyms();
        roots = GetWordsRoot();
    }
    public static string[] Content(List<string> allfiles)
    {
        //Recibir el contenido de cada archivo
        string[] result = new string[allfiles.Count];
        //Recorrer todos los archivos 
        for (int i = 0; i < allfiles.Count; i++)
        {
            //Leer cada archivo y guardar el contenido en el indice correspondiente de result 
            StreamReader text = new StreamReader(allfiles[i]);
            result[i] = text.ReadToEnd();


        }
        return result;

    }
    public static string[] Titles(List<string> allfiles)
    {
        //Recibir el titulo de cada archivo
        string[] result = new string[allfiles.Count];
        //recorrer los archivos
        for (int i = 0; i < allfiles.Count; i++)
        {
            //tomar el nombre del archivo
            string title = Path.GetFileNameWithoutExtension(allfiles[i]);
            //Añadir el titulo a result
            result[i] = title;
        }
        return result;
    }
    //guarda en un diccionario las raices con sus palabras correspondientes
    private static Dictionary<string, HashSet<string>> GetWordsRoot()
    {
        Dictionary<string, HashSet<string>> roots = new Dictionary<string, HashSet<string>>();
        Stemm raiz = new Stemm();
        foreach (string word in vocabulary.Keys)
        {
            string root = raiz.Stemming(word);
            if (!roots.ContainsKey(root))
            {
                roots.Add(root, new HashSet<string>());
                roots[root].Add(word);
            }
            else roots[root].Add(word);
        }
        return roots;
    }
    //guarda las palabras por archivo y sus indices, lo q me sirve tambien para saber cuantas veces se repiten
    private static Dictionary<int, Dictionary<string, HashSet<int>>> WordsInd()
    {
        Dictionary<int, Dictionary<string, HashSet<int>>> result = new Dictionary<int, Dictionary<string, HashSet<int>>>();
        for (int i = 0; i < content.Length; i++)
        {
            Dictionary<string, HashSet<int>> wordsInTitle = new Dictionary<string, HashSet<int>>();
            wordsInTitle = DivideAndKeepTrack(titles[i]);
            Dictionary<string, HashSet<int>> wordsinCont = new Dictionary<string, HashSet<int>>();
            wordsinCont = DivideAndKeepTrack(content[i]);
            foreach (var wordInd in wordsInTitle)
            {

                if (!wordsinCont.ContainsKey(wordInd.Key))
                {
                    wordsinCont.Add(wordInd.Key, wordInd.Value);
                }
                else
                {
                    wordsinCont[wordInd.Key].Union(wordInd.Value);
                }
            }
            
            result.Add(i, wordsinCont);
        }
        return result;
    }
    //caso particular del Divide pero teniendo en cuenta los indices
    public static Dictionary<string, HashSet<int>> DivideAndKeepTrack(string text)
    {
        //obtener las palabras limpias y sus indices  
        Dictionary<string, HashSet<int>> result = new Dictionary<string, HashSet<int>>();

        //guardar la palabra
        string word = "";
        //recorrer el texto
        for (int j = 0; j < text.Length; j++)
        {
            //verificar si me encuentro con un caracter q no sea letra o digito
            if (!char.IsLetterOrDigit(text[j]))
            {
                //verifica que si es un numero fraccionario no se corte con la coma o el punto 
                if ((text[j] == '.' || text[j] == ',') && (j + 1 < text.Length) ? char.IsDigit(text[j + 1]) : false && (j != 0) ? char.IsDigit(text[j - 1]) : false)
                {
                    word += ',';
                    continue;
                }

                //si existe una palabra antes de este caracter especial
                if (word != "")
                {
                    //si no la tengo agregada 
                    if (!result.ContainsKey(word))
                    {
                        //la guardo en mi diccionario (en minuscula) 
                        result.Add(word, new HashSet<int>());
                        //agrego su indice a la lista
                        result[word].Add(j - word.Length);
                        //y reinicializo la variable
                        word = "";

                    }
                    else
                    {
                        //agrego su indice a la lista
                        result[word].Add(j - word.Length);
                        //y reinicializo la variable
                        word = "";
                    }


                }


            }
            //Si me encuentro con una letra o digito la agrego a la palabra
            else word += text[j].ToString().ToLower();

        }
        //verificar la ultima palabra
        if (word != "")
        {
            //si no la tengo agregada 
            if (!result.ContainsKey(word.ToLower()))
            {
                //la guardo en mi diccionario (en minuscula) 
                result.Add(word.ToLower(), new HashSet<int>());

            }
            //agrego su indice a la lista
            result[word].Add(text.Length - word.Length);

        }
        return result;

    }
    //guarda todas las palabras q se encuentran en los documentos y los archivos en donde aparecen
    private static Dictionary<string, List<int>> Vocabulary()
    {
        Dictionary<string, List<int>> result = new Dictionary<string, List<int>>();
        List<string> seenWords = new List<string>();
        //recorre los archivos 
        for (int i = 0; i < allfiles.Count; i++)
        {
            //toma los diccionarios antes creados 
            foreach (var dic in wordsInd[i])
            {
                //si la palabra es nueva
                if (!seenWords.Contains(dic.Key))
                {
                    //toma la palabra y crea su lista de archivos correspondiente 
                    string word = dic.Key;
                    seenWords.Add(word);
                    List<int> arch = new List<int>();
                    //recorre de nuevo los archivos
                    for (int j = 0; j < allfiles.Count; j++)
                    {
                        //si el archivo contiene la palabra
                        if (wordsInd[j].ContainsKey(word))
                        {
                            //agrega el archivo a la lista
                            arch.Add(j);
                        }
                    }
                    //agrega la palabra y su lista de archivos 
                    result.Add(word, arch);

                }
            }
        }

        return result;

    }

    #endregion
    public static SearchResult Query(string query) 
    {
        queryRelevance = new Dictionary<string, float>[allfiles.Count];
        for (int i = 0; i < allfiles.Count; i++)//agregar los diccionarios al array de relevancia
        {
            queryRelevance[i] = new Dictionary<string, float>();
        }
        List<string> wordsInQuery = new List<string>();
        wordsInQuery = Divide(query);
      
        string suggestion = "";
        suggestion = SugBuild(wordsInQuery);

        List<string> wordsRoot = new List<string>();
        Stemm raiz = new Stemm();
        foreach (string word in wordsInQuery)
        {
            wordsRoot.Add(raiz.Stemming(word));
        }

        float[] score = new float[allfiles!.Count];  
        score = Score(wordsInQuery, wordsRoot);

        string[] snippet= new string[allfiles.Count];
        snippet = SnipBuild(score, content, wordsInQuery);

        SearchForOperators(query.ToLower(), allfiles, score, snippet);

        List<SearchItem> results = new List<SearchItem>();
        results = Results(score, snippet, suggestion);

        
        //para controlar la sugerencia
        if (suggestion == "*")
        {
            count++;
            if (count > 1) suggestion = "**";
        }
        return new SearchResult(results.ToArray(), suggestion);
    }
    #region Result
    //metodo para los resultados * la sugerencia es para saber si tengo q crear una respuesta
    private static List<SearchItem> Results(float[] score, string[] snippet, string suggestion)
    {
        List<SearchItem> results = new List<SearchItem>();  
        for (int i = 0; i < score.Length; i++)
        {
            if (score[i] > 0f)
            {
                results.Add(new SearchItem(titles[i], snippet[i], score[i]));
            }
        }

        Sort(results);
        if (results.Count == 0 && suggestion != "*")
        {
            results.Add(new SearchItem("No results Found😭", "", 0f)) ;
        }
        return results;
    }
    //metodo para ordenar en forma de burbuja los resultados
    private static void Sort(List<SearchItem> results)
    {
        bool finish = true;
        do
        {
            finish = true;
            for (int i = 0; i < results.Count - 1; i++)
            {
                if (results[i].Score < results[i + 1].Score)
                {
                    finish = false;
                    SearchItem item = results[i];
                    results.RemoveAt(i);
                    results.Insert(i + 1, item);
                }


            }
        }
        while (!finish);
    }
    //Construccion de la sugerencia
    private static string SugBuild(List<string> wordsInQuery)
    {
        string result = "";
        for(int i = 0; i < wordsInQuery.Count;i++ )
        {
            if (ValidWord(wordsInQuery[i]))
            {
                result += wordsInQuery[i] + " ";
            }
            else if (modifications.ContainsKey(wordsInQuery[i]))
            {
                string sugg = modifications[wordsInQuery[i]];
                result += sugg + " ";
                if (sugg != "") wordsInQuery[i] = sugg;
                
            }
            else
            {
                string sugg = NearWord(wordsInQuery[i]);
                if(!modifications.ContainsKey(wordsInQuery[i]))modifications.Add(wordsInQuery[i], sugg);
                result +=sugg + " ";
                if(sugg != "")wordsInQuery[i] = sugg;
            
            }
        }
        //saber si la query esta vacia
        if (result == "") return "*";
        //quita el espacio final y necesito q no me lo quite si no hay palabras
        if (result != " ") return result.Remove(result.Length-1);
        
         return result;
    }
    //detectar incontinencias y faltas de ortografia en la busqeda
    private static string NearWord(string word)
    {
        // busca la raiz primero y si existe busca la palabra derivada mas cercana
        Stemm raiz = new Stemm();
        string root = raiz.Stemming(word);
        if (root != "" && root != word)
        {
            if (roots.ContainsKey(root))
            {
                string derivedWord = "";
                foreach (string der in roots[root])
                {
                    if (Math.Abs(der.Length - word.Length) < Math.Abs(derivedWord.Length - word.Length)) derivedWord = der;

                }

                return derivedWord;
            }
        }
        //Busqueda de palabras distancia uno
        List<string> result = new List<string>();
       
        result = Edit(word);
        for(int i = 0; i < result.Count; i++)
        {
            if (ValidWord(result[i])) return result[i];
        }
        //Busqueda de palabras distancia dos
        List<string> result2 = new List<string>();
        for(int i = 0; i < result.Count; i++)
        {
            result2.AddRange(Edit(result[i]));
        }
        for(int i = 0; i < result2.Count; i++)
        {
            if(ValidWord(result2[i])) return result2[i];
        }
        string bestDerived = "";
        int bestDist = int.MaxValue;

        for(int i = word.Length - 2; i >= 3; i--)
        {
            if (roots.ContainsKey(word.Substring(0, i)))
            {
                string derivedWord = "";
                foreach (string der in roots[word.Substring(0, i)])
                {
                    if (Math.Abs(der.Length - word.Length) < Math.Abs(derivedWord.Length - word.Length)) derivedWord = der;

                }
                int dist = LevDist(derivedWord, word);
                if (dist < 5)return derivedWord;
                else if(dist < bestDist)
                {
                    bestDerived = derivedWord;
                    bestDist = dist;
                }
            }
            else
            {
                List<string> editedroots= new List<string>();
                editedroots = EditRoot(word.Substring(0,i));

                for (int j = 0; j < editedroots.Count; j++)
                {
                    if (roots.ContainsKey(editedroots[j]))
                    {
                        string derivedWord = "";
                        foreach (string der in roots[editedroots[j]])
                        {
                            if (Math.Abs(der.Length - word.Length) < Math.Abs(derivedWord.Length - word.Length)) derivedWord = der;

                        }

                        int dist = LevDist(derivedWord, word);
                        if (dist < 5) return derivedWord;
                        else if (dist < bestDist)
                        {
                            bestDerived = derivedWord;
                            bestDist = dist;
                        }
                    }
                }
            }
        }
   
        //devuelve la mejor aproximacion o nulo 
        return bestDerived;
    }
    // distancia de levenshtein 
    static int LevDist(string word1, string word2)
    {
        return LevDistRec(word1, word2, 0, 0);
    }
    private static int LevDistRec(string word, string word2, int v1, int v2)
    {
        if (v1 == word.Length) return word2.Length - v2;
        if (v2 == word2.Length) return word.Length - v1;
        if (word[v1] != word2[v2])
            return Math.Min(1 + Math.Min(1 + LevDistRec(word, word2, v1 + 1, v2), 1 + LevDistRec(word, word2, v1, v2 + 1)), 1 + LevDistRec(word, word2, v1 + 1, v2 + 1));

        return LevDistRec(word, word2, v1 + 1, v2 + 1);
    }
    //metodo para editar las palabras 
    private static List<string> Edit(string word)
    {
        List<string> result = new List<string>();

        for (int j = 0; j <= word.Length; j++)
        {
            if (j == word.Length)
            {
                foreach (var letter in alphabet)
                {
                    result.Add(word.Insert(j, letter.ToString()));
                }
            }
            else
            {
                foreach (var letter in alphabet)
                {
                    result.Add(word.Remove(j, 1).Insert(j, letter.ToString()));
                    result.Add(word.Insert(j, letter.ToString()));

                }
                result.Add(word.Remove(j, 1));
            }
        }    
        

        return result;
    }
    private static List<string> EditRoot(string word)
    {
        List<string> result = new List<string>();

        for (int j = 0; j < word.Length; j++)
        {

            foreach (var letter in alphabet)
            {
                result.Add(word.Remove(j, 1).Insert(j, letter.ToString()));
                result.Add(word.Insert(j, letter.ToString()));

            }
            result.Add(word.Remove(j, 1));
            
        }


        return result;
    }
   
    //Genera los snippets temporales ya q estos pueden ser cambiados por los operadores 
    private static string[] SnipBuild(float[] score, string[] content, List<string> wordsInQuery)
    {
        string[] result = new string[allfiles.Count];
        for (int i = 0; i < score.Length; i++)
        {
            if (score[i] > 0f)
            {
                string relevantWord = "";
                float wordRelevance = 0f;
                int index = -1;

                for (int j = 0; j < wordsInQuery.Count; j++)
                {

                    if (queryRelevance[i].ContainsKey(wordsInQuery[j]) && queryRelevance[i][wordsInQuery[j]] > wordRelevance)
                    {
                        relevantWord = wordsInQuery[j];
                        wordRelevance = queryRelevance[i][wordsInQuery[j]];
                        index = wordsInd[i][wordsInQuery[j]].Min();

                    }
                    
                }
                //para saber donde poner los ()
                if(index == 0) result[i] = SnipSubString(content[i], 0, index, index + relevantWord.Length,titles[i]);
                if (index < 300) result[i] = SnipSubString(content[i], 0, index - 1, index + relevantWord.Length,titles[i]);
                else
                {
                    int count = 300;

                    for (int j = index; count > 0; j--, count--)
                    {

                        if (content[i][j] == '.' && !Char.IsDigit(content[i][j + 1]))
                        {
                            result[i] = SnipSubString(content[i], j, index - 1, index + relevantWord.Length,titles[i]);
                            break;
                        }

                        else if (count == 1) result[i] = SnipSubString(content[i], j, index - 1, index + relevantWord.Length, titles[i]);
                    }

                }


            }
         
        }
        return result;
    }
    //metodo para al devolver el snippet encerrar en () la palabra mas importante
    private static string SnipSubString(string content,int index, int pos1, int pos2, string title)
    {
        string result = "";
        for(int i = index; i < content.Length && i - index < 500; i++)
        {
            if (i == pos1 && !char.IsLetterOrDigit(content[i]) && pos1 > title.Length) result += "(";
            if (i == pos2 && !char.IsLetterOrDigit(content[i])&& pos1 > title.Length) result += ")";
            else result+= content[i];

        }
        return result;
    }
    #endregion
    #region Operators
    //metodo para buscar en la query los operadores
    public static void SearchForOperators(string query, List<string> allfiles, float[] score, string[] snippet)
    {
        //busco los operadores en los lugares validos
        for (int i = 0; i < query.Length - 1; i++)
        {

            switch (query[i])
            {
                case '!':
                    ForbiddenWord(query, allfiles, i, score);
                    break;
                case '^':
                    RequiredWord(query, allfiles, i, score);
                    break;
                case '~':
                    WordsProximity(query, allfiles, i, score, snippet);
                    break;
                case '*':
                    Highlight(query, i, score, snippet);
                    break;

                default: break;

            }
        }
    }
    // metodo para el operador *
    private static void Highlight(string query, int i, float[] score, string[] snippet)
    {
        string word = WordInOp(query, i);

        if (word != "")
        {
            //sacar la cuenta de cuantos operadores * hay
            float count = 0f;
            for (int j = i; j > -1; j--)
            {
                if (query[j] == '*')
                {
                    count++;
                    continue;
                }
                break;
            }
            // ver la modificacion q le hice a la palabra si esta no es valida
            if (!ValidWord(word))
            {

                word = modifications[word];
            }

            for (int j = 0; j < score.Length; j++)
            {
                // en los documentos donde aparezca la palabra se revisa cual fue su relevancia y se multiplica por la cantidad de *
                if (queryRelevance[j].ContainsKey(word))
                {
                    score[j] += queryRelevance[j][word] * count;
                    // cambiar el snippet
                    int index = 0;
                    if (wordsInd[j][word].Min() > titles[j].Length)
                    {
                        index = wordsInd[j][word].Min();

                    }
                    else if (wordsInd[j][word].Max() > titles[j].Length)
                    {
                        index = wordsInd[j][word].Max();
                    }
                    snippet[j] = "*" + content[j].Substring(index, Valid(j, index) ? 500 : content[j].Length - index - 1);
                }
            }
        }
    }
    // metodo para el operador ~
    private static void WordsProximity(string query, List<string> allfiles, int i, float[] score, string[] snippet)
    {
        //llama a los metodos para buscar las palabras afectadas por los operadores
        string word1 = WordInOpBW(query, i);
        string word2 = WordInOp(query, i);

        //si ninguna de las dos palabras es vacia
        if (word1 != "" && word2 != "")
        {

            //verifica q las palabras sean validas
            if (!ValidWord(word1))
            {
                word1 = modifications[word1];
            }
            if (!ValidWord(word2))
            {
                word2 = modifications[word2];
            }
            if (word1 == word2) return;
            //recorre el score 
            for (int j = 0; j < score.Length; j++)
            {
                if (score[j] > 0f)
                {
                    HashSet<int> ind1 = new HashSet<int>();
                    HashSet<int> ind2 = new HashSet<int>();
                    //si existen las dos palabras
                    if (wordsInd[j].TryGetValue(word1, out ind1!) && wordsInd[j].TryGetValue(word2, out ind2!))
                    {


                        int dist = int.MaxValue;
                        int minInd = int.MinValue;
                        int maxInd = int.MaxValue;
                        // halla la menor distancia
                        foreach (int i1 in ind1)
                        {
                            foreach (int i2 in ind2)
                            {
                                
                                if (Math.Abs(i1 - i2) < Math.Abs(dist))
                                {
                                    dist = i2 - i1;
                                    if (i1 > i2)
                                    {
                                        maxInd = i1;
                                        minInd = i2;
                                        
                                    }
                                    else
                                    {
                                        maxInd = i2;
                                        minInd = i1;
                                    }
                                    
                                }
                            }
                        }
                        //tiene en cuenta si las palabras estan en orden
                        if (dist > 0)
                        {
                            score[j] *= 100 / dist;
                        }
                        if (dist < 0)
                        {
                            score[j] *= 75 / Math.Abs(dist);
                        }
                        //cambiar el snippet
                        string newSnip = "";
                        int k = minInd;
                        if (minInd < titles[j].Length) k = 0;
                        for (; k < content[j].Length; k++)
                        {
                            if (k > maxInd && content[j][k] == ' ')
                            {
                                break;
                            }
                            else if (k > minInd && content[j][k] == ' ')
                            {
                                newSnip += '~';
                            }
                            else newSnip += content[j][k];
                        }
                        snippet[j] = newSnip;
                    }
                    //descarta los documentos en donde no aparecen las dos palabras 
                    else score[j] = 0f;
                }

            }
        }

    }
    // metodo para el operador ^
    private static void RequiredWord(string query, List<string> allfiles, int i, float[] score)
    {
        string word = WordInOp(query, i);
        if (word != "")
        {
            // busca si la palabra existe en el vocabulario
            List<int> arch = new List<int>();
            if (vocabulary.TryGetValue(word, out arch!))
            {// saca los archivos 
                for (int j = 0; j < score.Length; j++)
                {//rrecorre todos los archivos y a los q no estan en mi lista le quito la relevancia
                    if (!arch.Contains(j)) score[j] = 0f;
                }
            }
            //en este caso la palabra no aparece en ningun archivo
            else if (Words.synonyms.ContainsKey(word)) score = new float[allfiles.Count];

            // si no busca en las modificaciones 
            else
            {
                word = modifications[word];
                if (word != "")
                {
                    if (vocabulary.TryGetValue(word, out arch!))
                    {
                        for (int j = 0; j < score.Length; j++)
                        {
                            if (!arch.Contains(j)) score[j] = 0f;
                        }
                    }
                }


            }
        }
    }
    // metodo para el operador !
    private static void ForbiddenWord(string query, List<string> allfiles, int i, float[] score)
    {
        string word = WordInOp(query, i);
        if (word != "")
        {
            if (vocabulary.ContainsKey(word))
            {
                foreach (var arch in vocabulary[word])
                {
                    score[arch] = 0f;
                }
            }
            //si la palabra no existe en mis archivos pues todo sigue igual
            else if (Words.synonyms.ContainsKey(word)) return;
            else
            {
                word = modifications[word];
                if (word != "")
                {
                    foreach (var arch in vocabulary[word])
                    {
                        score[arch] = 0f;
                    }
                }

            }
        }
    }

    #endregion
    #region TF-IDF

    //tf = cantidad de veces que se repite la palabra en el archivo/ cantidad de palabras en el archivo 
    //normalizarlo con la maxima frecuencia de una palabra en el texto
   
    //idf = log (cantidad de documentos/ cantidad de documentos en donde aparece la palabra) 
    //umbral del 95% para que el idf sea cero y del 90% pra que sea muy pobre
    //score de una palabra por archivo = tf*idf
    private static float Tf(string word, int arch)
    {

        int count = wordsInd[arch][word].Count;  
        int max = 0;
        foreach (var dic in wordsInd[arch])
        {
            if (dic.Value.Count > max) max = dic.Value.Count;
        }
        
        float tf = ((float)count / (float)max);
      
        return tf;
    }
    private static float Idf(string word, List<string> allfiles)
    {
        string input = word;
        int trueDocs = 0;
        trueDocs = vocabulary[word].Count;
        if ((float)trueDocs/(float)allfiles.Count > 0.95f) return 0f;
        if ((float)trueDocs / (float)allfiles.Count > 0.90f) return 0.01f;


        float idf = (float)Math.Log((float)allfiles.Count / (float)trueDocs);
        
        return idf;
    }
    private static float Tf_idf(string word, int arch, List<string> allfiles)
    {

        return Tf(word, arch) * Idf(word, allfiles);

    }
    static float[] Score(List<string> wordsInQuery, List<string> wordsRoot)
    {

        float[] score = new float[allfiles.Count];
        //leva la cuenta de cuantas palabras de la query tiene cada doc
        int[] count = new int[allfiles.Count];
        //lleva el mayor tf idf alcanzado por cada palabra 
        float[] wordsScore = new float[wordsInQuery.Count];
        //guarda las palabras derivadas y sinonimos para agrearlos a la query para tenerlas en cuenta en el snippet 
        List<string> wordsAdded = new List<string>();
        for (int j = 0; j < wordsInQuery.Count; j++)
        {
            //si la palabra aparece
            if (vocabulary.ContainsKey(wordsInQuery[j]))
            {
                foreach (var arch in vocabulary[wordsInQuery[j]])
                {
                    float relevance = 0f;
                    relevance = Tf_idf(wordsInQuery[j], arch, allfiles);
                    if(!queryRelevance[arch].ContainsKey(wordsInQuery[j]))queryRelevance[arch].Add(wordsInQuery[j], relevance);
                    // para que los score 0 no multipliquen palabras
                    if (relevance > 0f)
                    {
                        count[arch]++;
                        score[arch] += relevance;
                        //multiplico separado para q el orden de las palabras no importe 
                        score[arch] *= (float)count[arch];
                        // actualiza el score de la palabra
                        if (wordsScore[j] < relevance) wordsScore[j] = relevance;
                    }


                }

            }
            // si la palabra es relevante
            if (wordsScore[j] > 0f)
            {
                // busca sinonimos
                if (Words.synonyms.ContainsKey(wordsInQuery[j]))
                {
                    foreach (var syn in Words.synonyms[wordsInQuery[j]])
                    {

                        if (vocabulary.ContainsKey(syn))
                        {
                            //agrega el sinonimo
                            wordsAdded.Add(syn);
                            foreach (var arch in vocabulary[syn])
                            {

                                float relevance = 0f;
                                relevance = Tf_idf(syn, arch, allfiles) / 100f;
                                if (!queryRelevance[arch].ContainsKey(syn)) queryRelevance[arch].Add(syn, relevance);
                                //sumo 1/100 de la relevancia
                                score[arch] += relevance;
                                // no multiplico 
                            }

                        }

                    }

                }
                //busca las palabras derivadas
                if (roots.ContainsKey(wordsRoot[j]))
                {
                    foreach (string word in roots[wordsRoot[j]])
                    {
                        if(word != wordsInQuery[j])
                        {
                            //la agrega
                            wordsAdded.Add(word);
                            foreach (var arch in vocabulary[word])
                            {
                                float relevance = 0f;
                                relevance = Tf_idf(word, arch, allfiles) / 100f;
                                if (!queryRelevance[arch].ContainsKey(word)) queryRelevance[arch].Add(word, relevance);
                                score[arch] += relevance;

                            }
                        }
                       
                    }
                }



            }

        }
        //agrega las palabras adiciondas
        wordsInQuery.AddRange(wordsAdded);

        return score;
    }
    #endregion
    #region Metodos auxiliares
    //saber si la palabra se encuentra en mi diccioario local o en el de sinonimos
    private static bool ValidWord(string word)
    {
        if (vocabulary.ContainsKey(word) || Words.synonyms.ContainsKey(word)) return true;
        return false;
    }
    //determina si el del contenido del archivo i a partir de indice j se puede sacar un substring de tamanno 500  
    private static bool Valid(int i, int j)
    {
        return (content[i].Length - j - 1) > 500;
    }
   
    //metodo para separar las palabras de un texto
    public static List<string> Divide(string text)
    {
        //obtener las palabras limpias 
        List<string> result = new List<string>();
        //variable local creada a partir de la entrada del metodo
        string input = text;
        //guardar la palabra
        string word = "";
        //recorrer el texto
        for (int j = 0; j < input.Length; j++)
        {
            //verificar si me encuentro con un caracter q no sea letra o digito
            if (!char.IsLetterOrDigit(input[j]))
            {
                //verifica que si es un numero fraccionario no se corte con la coma o el punto 
                if ((text[j] == '.' || text[j] == ',') && (j + 1 < text.Length) ? char.IsDigit(text[j + 1]) : false && (j != 0) ? char.IsDigit(text[j - 1]) : false)
                {
                    word += ',';
                    continue;
                }
                //si existe una palabra antes de este caracter especial
                if (word != "")
                {
                    //guardarla en la lista (en minuscula para verificar que asi sea) 
                    result.Add(word.ToLower());
                    //y reinicializar la variable
                    word = "";
                }


            }
            //Si me encuentro con una letra o digito la agrego la palabra
            else word += input[j];

        }
        //verifica la ultima palabra
        if (word != "") result.Add(word.ToLower());

        return result;

    }
    //metodo auxiliar para saber la palabra afectada por el operando 
    private static string WordInOp(string query, int i)
    {

        string word = "";
        int j = 0;
        //obviar espacios
        for (int x = 1; x < query.Length - i; x++)
        {
            if (!(query[i + x] == ' '))
            {
                j = i + x;
                break;
            }

        }

        for (; j < query.Length; j++)
        {

            if (!char.IsLetterOrDigit(query[j]))
            {
                //verifica que si es un numero fraccionario no se corte con la coma o el punto 
                if ((query[j] == '.' || query[j] == ',') && (j + 1 < query.Length) ? char.IsDigit(query[j + 1]) : false && char.IsDigit(query[j - 1]))
                {
                    word += ',';
                    continue;
                }
                break;
            }
            word += query[j];
        }
        return word;
    }
    //caso particular para el operador ~ para hallar la palabra anterior
    private static string WordInOpBW(string query, int i)
    {
        //obviar espacios
        string word = "";
        int j = 0;
        int x = 1;
        for (; x < i; x++)
        {

            if (!(query[i - x] == ' '))
            {
                j = i - x;
                break;
            }

        }

        for (; j > 0; j--)
        {

            if (!char.IsLetterOrDigit(query[j]))
            {
                //verifica que si es un numero fraccionario no se corte con la coma o el punto 
                if ((query[j] == '.' || query[j] == ',') && char.IsDigit(query[j + 1]) && char.IsDigit(query[j - 1]))
                {
                    word += ',';
                    continue;
                }
                break;

            }

        }
        //pone el indicador donde empieza la palabra si es que no lo esta
        if (!char.IsLetterOrDigit(query[j]) && query[j] != '~') j++;
        // va desde la j hasta la x
        word = query.Substring(j, i - x - j + 1);
        return word;
    }
    #endregion
}

