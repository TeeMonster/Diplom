using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.ComponentModel;
using EntityFramework.Extensions;

namespace TestEntity
{
    class Program
    {
        class ParserCat
        {
            StartupEntities st;// = new StartupEntities();

            HttpWebRequest req;
            HttpWebResponse resp;
            StreamReader sr;
            private string site = "http://www.utkonos.ru";
            private string rootcat = "http://www.utkonos.ru/cat/1";
            private string stopstring = "Наше предложение";
            private string ish = "<a", zam = "~";
            private string sregecat = @"(?<=~)([^~]*)(align_center cat_preview)+([^~]*?)(?=</a>)";
            string sregecatname = @"(?<=>)[А-Я]+.*";
            string sregecaturl = @"(?<=href=).*?(?= )";

            public ParserCat()
            {
                site = "http://www.utkonos.ru";
            }

            public ParserCat(string in_site, string in_rootcat, string in_stopstring, string in_ish, string in_zam, string in_regecat, string in_regecatname, string in_regecaturl)
            {
                site = in_site;
                rootcat = in_rootcat;
                stopstring = in_stopstring;
                ish = in_ish;
                zam = in_zam;
                sregecat = in_regecat;
                sregecatname = in_regecatname;
                sregecaturl = in_regecaturl;
            }

            public void GoParse(string cat, string parentname)
            {
                req = (HttpWebRequest)WebRequest.Create(cat);
                resp = (HttpWebResponse)req.GetResponse();
                sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
                string content = sr.ReadToEnd();
                sr.Close();
                Movecat(content, parentname);
            }

            void Movecat(string content, string parent)
            {
                if (content.IndexOf(stopstring) != -1 || content.IndexOf("Новинки") != -1)//-1 в случае неудачи
                {
                    Regex recat = new Regex(sregecat);
                    Regex recatname = new Regex(sregecatname);
                    Regex recaturl = new Regex(sregecaturl);
                    content = content.Replace(ish, zam); // указывать в конфигурационном файле потом
                    MatchCollection matches = recat.Matches(content);
                    MatchCollection matchcatname;
                    MatchCollection matchcaturl;
                    string urlcat;
                    string catname;
                    foreach (Match mat in matches)
                    {
                        matchcaturl = recaturl.Matches(mat.Value);
                        urlcat = site + matchcaturl[0].Value.ToString().Trim('"');
                        matchcatname = recatname.Matches(mat.Value);
                        catname = matchcatname[0].Value;
                        Console.WriteLine(parent);
                        Console.WriteLine(catname);
                        Console.WriteLine(urlcat);
                        LoadToDB(parent,catname,urlcat);
                        //Потом каждую категорию на разбор подавать с проверкой товары там или нет.
                        GoParse(urlcat, catname);
                    }
                }
                else
                {
                }
            }
            //void UpdateToDB(string catname)
            //{
            //    using (st = new StartupEntities())
            //    {
            //        try
            //        {
            //            st.Category.Where(u => u.F_CAT_NAME == catname).Update(u => new Category { });
            //        }
            //        catch { }
            //    }
            //}

            void LoadToDB(string parent, string catname, string urlcat)
            {
                using (st = new StartupEntities())
                {
                    try
                    {
                        V_CATEGORY v_cat = new V_CATEGORY { F_CAT_NAME = catname, F_CAT_URL = urlcat, F_CAT_PARENT_NAME = parent };
                        st.V_CATEGORY.Add(v_cat);
                        st.SaveChanges();
                    }
                    catch { }
                }
            }
        }

        class ParserProduct
        {
            StartupEntities st;
            private string site = "http://www.utkonos.ru";
            string ish = "class=goods_view", zam = "~";
            string sregetmp= @"(?<=~)([^~]*)( class=""goods_weight"">)+([^~]*)(?=class=""el_form small btn disabled bradius_c-r reduce)";
            string sregename = @"(?<=title=)([^>]?)*(?=>)";
            string sregecost = @"(?<=price"" value="")([^=]?)*(?="">)";
            string sregeves = @"(?<=</i>)(\d+)*.*(?= кг)";
            string sregeurl = @"(?<=href="")([^=]?)*(?="">)";
            string sregenext = @"(?<=href=)([^=]?)*(?=>Вперед)";
            private string sregecat = @"(?<=~)([^~]*)(align_center cat_preview)+([^~]*?)(?=</a>)";
            public ParserProduct()
            {

            }

            decimal costf(string url)
            {
                HttpWebRequest req;
                HttpWebResponse resp;
                StreamReader sr;
                string content;
                req = (HttpWebRequest)WebRequest.Create(url);
                resp = (HttpWebResponse)req.GetResponse();
                sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
                content = sr.ReadToEnd();
                sr.Close();
                Regex regecost = new Regex(sregecost);
                Match m = regecost.Match(content);
                decimal cost = 0;
                if (m.Success) { cost = Convert.ToDecimal(m.Value.Replace('.',',')); }
                return cost;
            }

            public void ParseCost()
            {
                st = new StartupEntities();
                StartupEntities othercontext = new StartupEntities();
                var q = from p in st.Product
                        select p; //st.Product.SqlQuery("select * from Product");
                Console.WriteLine(q.Count());
                //Parallel.For()  ParallelOptions.MaxDegreeOfParallelism
                ParallelOptions op = new ParallelOptions();
                op.MaxDegreeOfParallelism = 2;

                foreach (Product p in q)
                {
                    if (p.F_URL!="SYSTEM")
                    othercontext.P_INS_COSTPRODUCT(Convert.ToInt32(p.F_PRODUCT_ID), costf(p.F_URL));
                    Console.WriteLine(p.F_URL);
                }
                //Parallel.ForEach(q, op, c =>
                //{   
                //    othercontext.P_INS_COSTPRODUCT(Convert.ToInt32(c.F_PRODUCT_ID), costf(c.F_URL));
                //});
            }

            public void GoFirstParse()
            {
                using (st = new StartupEntities())
                {
                    /* звездочка в запросе */
                  var q= st.Category.SqlQuery("select * from Category c where not exists(select '+' from Category where F_PARENT_CAT_ID=c.F_CAT_ID)");
                  //Parallel.ForEach(q, c =>
                  //{
                  //  LoadToDB(Parse(c.F_CAT_URL, c.F_CAT_ID));
                  //});
                  foreach (Category c in q)
                  {
                      LoadToDB(Parse(c.F_CAT_URL, c.F_CAT_ID));
                  }
                }
            }
            
            void LoadToDB(List<Product> lp)
            {
                using (st = new StartupEntities())
                {
                    st.Product.AddRange(lp);
                    st.SaveChanges();
                }
            }

            List<Product> Parse(string urlcat, int cat_id)
            {
                List<Product> lp = new List<Product>();
                HttpWebRequest req;
                HttpWebResponse resp;
                StreamReader sr;
                req = (HttpWebRequest)WebRequest.Create(urlcat);
                resp = (HttpWebResponse)req.GetResponse();
                sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
                string content = sr.ReadToEnd();
                sr.Close();
                /*парсим товары название цена вес ссылку на товар*/
                content = content.Replace(@"class=""goods_view", "~");
                Regex retmp = new Regex(sregetmp);
                Regex rename = new Regex(sregename);
                Regex reves = new Regex(sregeves);
                Regex reurl = new Regex(sregeurl);
                MatchCollection matchtovar = retmp.Matches(content);
                Match matchurl;
                //Match matchcost;
                Match matchname;
                Match matchves;
                foreach (Match mattovar in matchtovar)
                {
                    matchurl = reurl.Match(mattovar.Value);
                    matchname = rename.Match(mattovar.Value);
                    matchves = reves.Match(mattovar.Value);
                    Product p = new Product { F_CAT_ID = cat_id, F_NAME = matchname.Value.Trim('"').Trim('\''), F_URL = site + matchurl.Value, F_WEIGHT = Convert.ToDecimal(matchves.Value) };
                    lp.Add(p);
                }
                Regex renext = new Regex(sregenext);
                MatchCollection matchnexturl= renext.Matches(content);
                foreach (Match mat in matchnexturl)
                {
                    if (mat.Value != null)
                    {
                        string urlnext = site + mat.Value.Trim('"');
                        Console.WriteLine(urlnext);
                        lp.AddRange(Parse(urlnext, cat_id));
                    }
                }
                return lp;
                /*достаем ссылку на следующую страницу категории, и скармливаем ее если ее нет конец */
            }
        }

        class ParserRecipes
        {
            StartupEntities st;
            private string site = "http://daily-menu.ru";
            string sregenexturl = @"(?<=<li class=""next""><a href="")([^<]?)*(?="">)";
            //string sregerecipeurl = @"(?<=<article class=""recipe_anounce""><a href="")([^<]?)*(?="">)";
            string sregerecipeurl=@"(?<=<a href="")([^<]?)*(?=""><img class=""recipe_preview"")";
            string sregerecipename = @"(?<=<meta name=""og:title"" content="")([^<]?)*(?="" />)";
            string sregeingredients = @"(?<=itemprop=""ingredients"">)([^<]?)*(?=</td>)";
            string sregeingredientsweight = @"(?<=class=""variable"">)([0-9,\.]|-)+(?=</td)";
            string sregetmpnutr = @"(?<=На 1 порцию:</strong></td>)([^Н]?)*(?=<td><strong>На 100)";
            string sregenutritional = @"(?<=<strong>).*(?=</strong>)";

            public ParserRecipes()
            {

            }

            List<string> GetURLs(string urlcat)
            {
                List<string> res = new List<string>();
                HttpWebRequest req;
                HttpWebResponse resp;
                StreamReader sr;
                req = (HttpWebRequest)WebRequest.Create(urlcat);
                resp = (HttpWebResponse)req.GetResponse();
                sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
                string content = sr.ReadToEnd();
                sr.Close();
                Regex regenexturl = new Regex(sregenexturl);
                Match matchnexturl = regenexturl.Match(content);
                string snexturl=site+matchnexturl.Value;
                if (matchnexturl.Success) { res.Add(snexturl); res.AddRange(GetURLs(snexturl)); } else { snexturl = null; }
                return res;
            }

            List<Rec> ParseRecipes(int icatid, string urlcat)
            {
                List<Rec> lr = new List<Rec>();
                HttpWebRequest req;
                HttpWebResponse resp;
                StreamReader sr;
                req = (HttpWebRequest)WebRequest.Create(urlcat);
                resp = (HttpWebResponse)req.GetResponse();
                sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
                string content = sr.ReadToEnd();
                sr.Close();
                Regex regerecipeurl = new Regex(sregerecipeurl);
                MatchCollection matchesrecurl = regerecipeurl.Matches(content);
                string surlrecipe;
                Console.WriteLine(matchesrecurl.Count);
                foreach(Match m in matchesrecurl)
                {
                    surlrecipe = site + m.Value;
                    lr.Add(GetRecipe(surlrecipe, icatid));
                    //Console.ReadLine();
                }
                ///lr.AddRange(ParseRecipes(icatid, matchnexturl.Value));
                return lr;
            }

            struct Rec{
                   public Recipes r;
                   public List<Ingredients> i;
                   public List<Structure> str;
             }

            Rec GetRecipe(string urlrecipe, int icatid)
            {
                Console.WriteLine("зашло в  гет рецепт");
                Rec st = new Rec();
                st.str = new List<Structure>();
                st.i = new List<Ingredients>();
                HttpWebRequest req;
                HttpWebResponse resp;
                StreamReader sr;
                req = (HttpWebRequest)WebRequest.Create(urlrecipe);
                resp = (HttpWebResponse)req.GetResponse();
                sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
                string content = sr.ReadToEnd();
                sr.Close();
                Regex regerecname = new Regex(sregerecipename);
                Match mrecname = regerecname.Match(content);
                Regex regetmpnutr = new Regex(sregetmpnutr);
                Match mtmp = regetmpnutr.Match(content);
                Regex regenutritional = new Regex(sregenutritional);
                MatchCollection mnutritional = regenutritional.Matches(mtmp.Value);
                Console.WriteLine(mtmp.Value);
                Recipes rec = new Recipes { F_URL = urlrecipe, F_CAT_ID = icatid, F_NAME = mrecname.Value, F_CALORIES = mnutritional[1].Value == "-" ? 0 : Convert.ToDecimal(mnutritional[1].Value.Replace('.', ',')), F_PROTEINS = mnutritional[2].Value == "-" ? 0 : Convert.ToDecimal(mnutritional[2].Value.Replace('.', ',')), F_FATS = mnutritional[3].Value == "-" ? 0 : Convert.ToDecimal(mnutritional[3].Value.Replace('.', ',')), F_CARBONYDRATES = mnutritional[4].Value == "-" ? 0 : Convert.ToDecimal(mnutritional[4].Value.Replace('.', ',')) };
                st.r = rec;
                Console.WriteLine(rec.F_URL+" "+rec.F_CAT_ID.ToString()+" "+rec.F_NAME+" "+rec.F_CALORIES.ToString()+rec.F_PROTEINS.ToString()+" "+rec.F_FATS.ToString()+" "+rec.F_CARBONYDRATES.ToString());
                Regex regeingredients = new Regex(sregeingredients);
                MatchCollection matchingr = regeingredients.Matches(content);
                Regex regeingrweight = new Regex(sregeingredientsweight);
                MatchCollection matchweight = regeingrweight.Matches(content);
                foreach(Match m in matchweight)
                {
                    Console.WriteLine(m.Value);
                }
                int i=0;
                foreach(Match m in matchingr)
                {
                    st.i.Add(new Ingredients { F_NAME=m.Value});
                    st.str.Add(new Structure { F_WEIGHT = Convert.ToDecimal(matchweight[i * 5].Value.Replace('.',','))});

                    //Console.WriteLine(m.Value);
                    //Console.WriteLine(matchweight[i*5].Value);
                    i++;
                }
                //Console.ReadLine();
                
                return st;
            }

            int LoadToDBRec(Recipes rec)
            {
                int id=-1;
                using(st=new StartupEntities())
                {
                    var q = from p in st.Recipes
                            where p.F_NAME == rec.F_NAME && p.F_CARBONYDRATES == rec.F_CARBONYDRATES && p.F_PROTEINS == rec.F_PROTEINS
                            select p.F_RECIPE_ID;
                    if (q.Count() == 0)
                    {
                        st.Recipes.Add(rec);
                        st.SaveChanges();
                        q = from p in st.Recipes
                                where p.F_NAME == rec.F_NAME && p.F_CARBONYDRATES == rec.F_CARBONYDRATES && p.F_PROTEINS == rec.F_PROTEINS
                                select p.F_RECIPE_ID;
                        id = q.First(x => 1 == 1);
                    }
                }
                return id;
            }

            void LoadToDBIngredients(ref List<Ingredients> li)
            {
                using (st = new StartupEntities())
                {
                    for (int i = 0; i < li.Count;i++)
                    //foreach (Ingredients i in li)
                    {
                        Ingredients t = new Ingredients();
                        Ingredients ti = new Ingredients();
                        t = li[i];
                        ti = st.Ingredients.Where(x => x.F_NAME == t.F_NAME).DefaultIfEmpty().First();
                        if (ti == null)
                        {
                            Console.WriteLine("ti is null");
                            st.Ingredients.Add(li[i]);
                            st.SaveChanges();
                            ti = st.Ingredients.Where(x => x.F_NAME == t.F_NAME).DefaultIfEmpty().First();
                        }
                        else
                        {
                            Console.WriteLine("ti is not null");
                        }
                        li[i] = ti;
                    }
                }
            }

            void LoadToDBStruct(Rec rec)
            {
                using(st= new StartupEntities())
                {
                    var q = from p in st.Structure
                            where p.F_RECIPE_ID == rec.r.F_RECIPE_ID
                            select p;
                    if (q.Count() == 0)
                    {
                        for (int i = 0; i < rec.str.Count; i++)
                        {
                            rec.str[i].F_RECIPE_ID = rec.r.F_RECIPE_ID;
                            rec.str[i].F_INGREDIENT_ID = rec.i[i].F_INGREDIENT_ID;
                            Console.WriteLine(rec.str[i].F_RECIPE_ID);
                            Console.WriteLine(rec.str[i].F_INGREDIENT_ID);
                            st.Structure.Add(rec.str[i]);
                        }
                        st.SaveChanges();
                    }
                }
            }

            void LoadToDB(List<Rec> lr)
            {
                for (int i = 0; i < lr.Count;i++ )
                {
                    Rec r = new Rec();
                    r = lr[i];
                    r.r.F_RECIPE_ID=LoadToDBRec(r.r);
                    if (r.r.F_RECIPE_ID != -1)
                    {
                        LoadToDBIngredients(ref r.i);
                        LoadToDBStruct(r);
                    }
                }
            }

            public void GoPasre()
            {
                using (st = new StartupEntities())
                {
                    var q = from p in st.CategoryRecipes
                            select p;
                    foreach (CategoryRecipes c in q)
                    {
                        List<string> lg = GetURLs(c.F_CAT_URL);
                        //Console.WriteLine("catid="+c.F_CATEGORY_ID.ToString());
                        foreach (string s in lg)
                        {
                            LoadToDB(ParseRecipes(c.F_CATEGORY_ID, s));
                        }
                    }
                    //Parallel.ForEach(q, c => {
                    //    List<string> lg = GetURLs(c.F_CAT_URL);
                    //    foreach(string s in lg)
                    //    {
                    //        LoadToDB(ParseRecipes(c.F_CATEGORY_ID, s));
                    //    }
                    //});
                }
            }
        }

        class MatchIngrAndProduct
        {
            StartupEntities st;
            IQueryable<Product> lp;
            IQueryable<Ingredients> li;
            //List<Product> lp;
            //List<Ingredients> li;
            public MatchIngrAndProduct()
            {
                //lp = new List<Product>();
                //li = new List<Ingredients>();
            }

            bool M(Ingredients i, Product p)
            {
                bool res=true;
                string [] mi= i.F_NAME.Split();
                foreach(string s in mi)
                {
                    if (p.F_NAME.ToUpper().IndexOf(s.ToUpper())==-1)
                    {
                        res = false;
                        break;
                    }
                }
                return res;
            }

            public void Match()
            {
                int count = 3;
                using (st = new StartupEntities())
                {
                    li = from p in st.Ingredients
                         where p.F_NAME == "Огурцы"
                         orderby p.F_NAME.Length descending
                         select p;
                    lp = from p in st.Product
                         select p;
                    StartupEntities othercontext = new StartupEntities();
                    foreach (Ingredients i in li)
                    {
                        //List<> res = new List<>();
                        foreach (Product p in lp)
                        {
                            if (M(i, p))
                            {
                                Console.WriteLine(p.F_NAME);
                                count++;
                                //st.Category.SqlQuery("insert into ingredientproduct values ("+i.F_INGREDIENT_ID.ToString()+","+p.F_PRODUCT_ID.ToString()+")");
                                othercontext.PV_INS_INGREDIENTPRODUCT(i.F_INGREDIENT_ID, p.F_PRODUCT_ID);
                            }
                        }
                        othercontext.SaveChanges();
                        Console.WriteLine(i.F_NAME);
                    }
                    Console.WriteLine(count);
                }
            }
        }

        class SimplexTable
        {
            List<List<double>> table;
            double[][] tabletmp;
            bool solved;
            int INDrow_c;
            int INDcol_c;
            public List<int> bvar;
            public List<double> X;
            public double Price;
            int c_iter;
            bool max;
            public SimplexTable(double [][] A, double[] B, double [] C, char[]Znak, bool max)
            {
                c_iter = 0;
                X = new List<double>();
                solved = false;
                INDrow_c = -1;
                INDcol_c = -1;
                table = new List<List<double>>();
                bvar = new List<int>();
                //вводим доп. переменные, чтобы получить равенства
                int dopvar=0;
                for (int i=0;i<Znak.Length;i++)
                    if (Znak[i]!='=') dopvar++;
                //заполним Z в начало, чтобы его не потерять, при добавлении новых усоовий - для задачи максимизации!
                if (max)
                    for (int i = 0; i < C.Length; i++)
                        C[i] = -C[i];
                List<double> lc = new List<double>();
                lc.Add(0);
                table.Add(lc);
                table[0].AddRange(C.ToList());
                double[] dop_c = new double[dopvar];
                table[0].AddRange(dop_c.ToList());

                int count_dop_var = 0;
                //A подалось просто, надо дополнить переменные
                for (int i = 0; i < A.GetLength(0); i++)
                {
                    List<double> lb = new List<double>();
                    lb.Add(B[i]);
                    table.Add(lb); //Свободные члены добавили 0-ая колонка, чтобы не потерять
                    table[i+1].AddRange(A[i].ToList());
                    dop_c= new double[dopvar];//не уверен что тут нули!
                    if (Znak[i] != '=')
                    {   
                        dop_c[count_dop_var] = Znak[i] == '>' ? -1 : 1;
                        count_dop_var++;
                    }
                    table[i+1].AddRange(dop_c.ToList());
                    if (Znak[i]=='>')
                        for (int j = 0; j < table[i + 1].Count();j++)
                        {
                            table[i + 1][j] = -table[i + 1][j];
                        }
                    bvar.Add(i + A[0].Length+1);
                }
                for (int i = 0; i < table.Count(); i++)
                {
                    for (int j = 0; j < table[i].Count(); j++)
                        Console.Write(table[i][j].ToString()+" ");
                    Console.WriteLine();
                }
                Console.WriteLine();
                Console.ReadLine();
                
            }

            void SearchVed()
            {
                double tmp = 0;
                INDcol_c = -1;
                INDrow_c = -1;
                //проверяем свободные члены на отрицательность - ищем ведущую строку
                for (int i = 1; i < table.Count(); i++)
                    if (table[i][0] < tmp) { tmp = table[i][0]; INDrow_c = i; }
                if (INDrow_c != -1)
                {
                    //если есть отрицательные, ищем наименьшее, затем ищем ведущий столбец - наименьшее отрицательное значение в строке
                    tmp = 0;
                    for (int i = 1; i < table[INDrow_c].Count; i++)
                        if (table[INDrow_c][i] < tmp) { tmp = table[INDrow_c][i]; INDcol_c = i; }
                    if (INDcol_c == -1) { Console.WriteLine("INDcol_c ==-1"); Console.WriteLine("Система несовместна"); solved = true; Console.ReadLine(); }
                }
                else
                {
                    //свободные члены не отрицательные, ищем сначала ведущий столбец
                    tmp = 0;
                    for (int i = 1; i < table[0].Count(); i++)
                        if (table[0][i] < tmp) { tmp = table[0][i]; INDcol_c = i; } 
                    if (INDcol_c != -1)
                    {
                        //ищем ведущую строку
                        double o = double.PositiveInfinity;
                        for (int i = 1; i < table.Count; i++)
                        {

                            double bi = table[i][0];
                            double aik = table[i][INDcol_c];
                            if (bi != 0 && aik != 0 && aik * bi > 0)
                            {
                                tmp = Math.Abs(bi / aik);
                                if (o > tmp) { o = tmp; INDrow_c = i; }
                            }
                            else
                            {
                                if (bi == 0 && aik > 0)
                                {
                                    o = 0;
                                    INDrow_c = i;
                                    //возможно есть еще один вариант с нулем!
                                }
                            }
                        }
                    }
                }
                Console.WriteLine("Ведущая строка="+INDrow_c);
                Console.WriteLine("Ведущий столбец="+INDcol_c);
            }

            void Jordan()
            {
                tabletmp = new double[table.Count()][];
                for (int i = 0; i < table.Count(); i++)
                {
                    tabletmp[i] = new double[table[i].Count()];
                    tabletmp[i] = table[i].ToArray();
                }
                //подгтовка
                bvar[INDrow_c - 1] = INDcol_c; //заменили базисную переменную
                for (int i = 0; i < table.Count(); i++)
                {
                    if (i != INDrow_c) table[i][INDcol_c] = 0;
                }
                for (int i = 0; i < table[1].Count; i++)
                {
                    if (i != INDcol_c) table[INDrow_c][i] = tabletmp[INDrow_c][i] / tabletmp[INDrow_c][INDcol_c];
                }
                table[INDrow_c][INDcol_c] = 1;
                //схема жордана гаусса
                for (int i = 0; i < table.Count(); i++)
                {
                    if (i != INDrow_c)
                        for (int j = 0; j < table[1].Count(); j++)
                        {
                            if (j != INDcol_c)
                                table[i][j] = (tabletmp[i][j] * tabletmp[INDrow_c][INDcol_c] - tabletmp[i][INDcol_c] * tabletmp[INDrow_c][j]) / tabletmp[INDrow_c][INDcol_c];
                        }
                }
            }

            public void Simplex()
            {
                c_iter++;
                if (!solved)
                {
                    SearchVed();
                    if (INDrow_c==-1 || INDcol_c==-1)
                    {
                        solved = true;
                        Console.WriteLine(INDrow_c);
                        Console.WriteLine(INDcol_c);
                        Console.WriteLine(c_iter);
                        Console.ReadLine();
                        Price = max ? table[0][0]: -table[0][0];
                        for (int i = 1; i < table.Count(); i++)
                            X.Add(table[i][0]);
                    }
                    else
                    {
                        Jordan();
                        for (int i = 0; i < table.Count(); i++)
                        {
                            for (int j = 0; j < table[i].Count(); j++)
                                Console.Write("{0, 6} ", table[i][j]);
                            Console.WriteLine();
                        }
                        Console.WriteLine();
                        for (int i = 0; i < bvar.Count; i++)
                            Console.Write(bvar[i]+" ");
                            Console.WriteLine();
                        //Console.ReadLine();
                        Simplex();
                    }
                }
            }
        }

        class ForTest
        {
            StartupEntities st;
            public ForTest()
            {
                
            }

            public void Test()
            {
                double[][] A;
                double[] C;
                double[] B;
                char[] Znak;
               int user_id=1;
               using (st = new StartupEntities())
               {
                   var q = from p in st.F_GET_COST_RECIPES(DateTime.Now, user_id)
                           join x in st.Recipes on p.out_recipe_id equals x.F_RECIPE_ID
                           select new {Name=x.F_NAME, Cost=p.out_recipe_cost, PROTEINS=x.F_PROTEINS, U=x.F_CARBONYDRATES, F=x.F_FATS, C=x.F_CALORIES };
                   int count = q.Count();
                   Console.WriteLine(count);
                   A= new double[10][];
                   C= new double[count];
                   B= new double[10];
                   Znak= new char[10];
                   for (int i=0;i<10;i++) A[i]=new double[count];
                   int iter=0;
                   //foreach (var a in q)
                   var c = q.ToList();
                   for (int i = 0; i < count;i++)
                   {
                       var a = c[i];
                       A[0][iter] = Convert.ToDouble(a.F);
                       A[1][iter] = Convert.ToDouble(a.F);
                       A[2][iter] = Convert.ToDouble(a.PROTEINS);
                       A[3][iter] = Convert.ToDouble(a.PROTEINS);
                       A[4][iter] = Convert.ToDouble(a.U);
                       A[5][iter] = Convert.ToDouble(a.U);
                       A[6][iter] = Convert.ToDouble(a.C);
                       A[7][iter] = Convert.ToDouble(a.C);
                       A[8][iter] = Convert.ToDouble(1);
                       A[9][iter] = Convert.ToDouble(1);
                       C[iter] = Convert.ToDouble(a.Cost);
                       iter++;
                   }
                   Znak[0]='>';
                   Znak[1]='<';
                   Znak[2]='>';
                   Znak[3]='<';
                   Znak[4]='>';
                   Znak[5]='<';
                   Znak[6]='>';
                   Znak[7]='<';
                   Znak[8]='>';
                   Znak[9]='<';
                   B[0]=147; B[1]=245; B[2]=651; B[3]=980; B[4]=980;B[5]=1400;B[6]=9100;B[7]=10200;B[8]=21;B[9]=21;
               }
               SimplexTable simpl = new SimplexTable(A, B, C, Znak, false);
               simpl.Simplex();
               for (int i = 0; i < simpl.bvar.Count(); i++)
               {
                   Console.WriteLine("X" + simpl.bvar[i] + "=" + simpl.X[i]);
               }
               Console.WriteLine(simpl.Price);
            }
        }

        static void Main(string[] args)
        {
            //double[] C = new double[2]{-2,-3};//[2] { 14, 8 };
            //double[] B = new double[4]{18,16,5,21};//[3] { 2, 7, 7 };
            //char[] Znak = new char[4] { '<', '<', '<', '<' };//[3] { '>', '>', '<' };
            //double[][] A = new double[4][];//[3][];
            //A[0] = new double[2]{1,3};//[2] { -2, 1 };
            //A[1] = new double[2]{2,1};//[2] { 3, 1 };
            //A[2] = new double[2]{0,1};//[2] { 3, 1 };
            //A[3]= new double[2]{3,0};
            //SimplexTable st = new SimplexTable(A, B, C, Znak);
            //st.Simplex();
            //for (int i = 0; i < st.bvar.Count(); i++)
            //{
            //    Console.WriteLine("X" + st.bvar[i] + "=" + st.X[i]);
            //}
            //MatchIngrAndProduct m = new MatchIngrAndProduct();
            //m.Match();
            //ParserCat pc = new ParserCat();
            //pc.GoParse("http://www.utkonos.ru/cat/1", "");
            //ParserProduct pc = new ParserProduct();
            //pc.ParseCost();
            ForTest fr = new ForTest();
            fr.Test();
            //ParserRecipes pr = new ParserRecipes();
            //pr.GoPasre();
            //Console.WriteLine(content);
            //Console.ReadLine();
            //string filename;
            //filename=Console.ReadLine();
            //FileInfo f= new FileInfo(filename+".txt");
            //StreamWriter sw=f.CreateText();
            //sw.Write(content);
            //FileStream sw = new FileStream(@"D:\учеба\5 курс\Диплом\Обкатка\" + filename + ".txt", FileMode.Create);
            //sw.Close();
            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}
