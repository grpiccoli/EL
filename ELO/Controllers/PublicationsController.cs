using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ELO.Models;
using HtmlAgilityPack;
using System.Net.Http;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Text.RegularExpressions;
using ELO.Data;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading;
using Microsoft.AspNetCore.NodeServices;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace ELO.Controllers
{
    [Authorize]
    public class PublicationsController : Controller
    {  
        private readonly ApplicationDbContext _context;

        public PublicationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Publications
        [AllowAnonymous]
        public async Task<IActionResult> Index(
            int? pg, //page
            int? trpp, //results per page
            string srt, //value to sort by
            bool? asc, //ascending or descending sort
            string[] val, //array of filter:value
            string[] src, //List of engines to search
            string q, //search value
            string sp,
            [FromServices] INodeServices nodeServices)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            #region Variables
            if (!pg.HasValue) pg = 1;
            if (!trpp.HasValue) trpp = 20;
            if (!asc.HasValue) asc = true;
            if (srt == null) srt = "source";
            var order = asc.Value ? "asc" : "desc";

            ViewData[nameof(src)] = src;
            ViewData["srcs"] = string.Join(",", src);
            ViewData[nameof(srt)] = srt;
            ViewData[nameof(pg)] = pg;
            ViewData[nameof(q)] = q;
            ViewData[nameof(asc)] = asc;
            ViewData[nameof(trpp)] = trpp;
            ViewData[nameof(sp)] = sp;
            ViewData["any"] = false;

            IEnumerable<Publication> publications = new List<Publication>();
            #endregion

            #region universities dictionary
            var ues = new Dictionary<string, string>()
            {
                {"uchile", "Universidad de Chile"},
                {"ula", "Universidad Los Lagos"},
                {"utal","Universidad de Talca"},
                {"umag","Universidad de Magallanes"},
                {"ust", "Universidad Santo Tomás"},
                {"ucsc","Universidad Católica de la Santísima Concepción"},
                {"uct","Universidad Católica de Temuco"},
                {"uach","Universidad Austral de Chile"},
                {"udec","Universidad de Concepción"},
                {"pucv","Pontificia Universidad Católica de Valparaíso"},
                {"puc","Pontificia Universidad Católica"},
            };
            #endregion
            #region diccionario Proyectos conicyt
            var conicyt = new Dictionary<string, string>()
            {
                {"FONDECYT","Fondo Nacional de Desarrollo Científico y Tecnológico"},
                {"FONDEF","Fondo de Fomento al Desarrollo Científico y Tecnológico"},
                {"FONDAP","Fondo de Financiamiento de Centros de Investigación en Áreas Prioritarias"},
                {"PIA","Programa de Investigación Asociativa"},
                {"REGIONAL","Programa Regional de Investigación Científica y Tecnológica"},
                {"BECAS","Programa Regional de Investigación Científica y Tecnológica"},
                {"CONICYT","Programa Regional de Investigación Científica y Tecnológica"},
                {"PROYECTOS","Programa Regional de Investigación Científica y Tecnológica"},
            };
            #endregion
            #region diccionario de Proyectos
            Dictionary<string, string> proj = conicyt.Concat(new Dictionary<string, string>() {
                //{"FAP","Fondo de Administración Pesquero"},//"subpesca"
                {"FIPA","Fondo de Investigación Pesquera y de Acuicultura"},//"subpesca"
                {"CORFO","Corporación de Fomento a la Producción"}//"corfo"
            }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            #endregion
            #region Artículos Indexados
            var gs = new Dictionary<string, string>()
            {{"gscholar","Google Scholar"}};
            #endregion
            #region Patentes
            var gp = new Dictionary<string, string>()
            {{"gpatents","Google Patents" }};
            #endregion

            ViewData[nameof(ues)] = ues;
            ViewData[nameof(proj)] = proj;
            ViewData[nameof(gs)] = gs;
            ViewData[nameof(gp)] = gp;

            if (!string.IsNullOrWhiteSpace(q))
            {
                var sps = sp == "la" ? "Loxechinus albus" : "Concholepas concholepas";
                q += " " + sps;
                Regex ress = new Regex(@"\d+");
                Regex res = new Regex(@"(\d+\.*\d+)(?!.*\d)");
                Regex sep = new Regex(@",");

                Dictionary<string, int> NoResults = new Dictionary<string, int> { };
                Dictionary<string, int> NoProjs = new Dictionary<string, int> { };
                Dictionary<string, int> NoArticles = new Dictionary<string, int> { };
                Dictionary<string, int> NoPatents = new Dictionary<string, int> { };

                var rpp = (int)Math.Ceiling((double)trpp.Value / src.Count());
                var srt_utal = srt;
                string sort_by, srt_uach, u, rep;
                int srt_uct, ggl;
                string doi = "https://dx.doi.org/";

                List<Publication> Publications = new List<Publication>();

                switch (srt)
                {
                    case "title":
                        sort_by = "dc.title_sort";
                        srt_uct = 1;
                        ggl = 0;
                        srt_uach = "ftitre";
                        break;
                    case "date":
                        sort_by = "dc.date.issued_dt";
                        srt_uct = 2;
                        ggl = 1;
                        srt_uach = "udate";
                        break;
                    default:
                        sort_by = "score";
                        srt_utal = "rnk";
                        ggl = srt_uct = 0;
                        srt_uach = "sdxscore";
                        break;
                }
                #region REPOSITORIO UChile
                u = "uchile";
                if (src.Contains(u))
                {
                    //23s
                    //sort_by   dc.date.issued_dt   dc.title_sort   score
                    //order     asc                 desc
                    rep = $"http://repositorio.uchile.cl/discover?query={q}&rpp={rpp}&page={pg}&sort_by={sort_by}&order={order}";

                    var co = _context.Company.SingleOrDefault(c => c.Acronym == u);
                    try
                    {
                        HttpClient hc = new HttpClient();
                        HttpResponseMessage result = await hc.GetAsync(rep);
                        HtmlDocument doc = new HtmlDocument();
                        doc.Load(await result.Content.ReadAsStreamAsync());
                        string tmp = ress.Matches(doc.DocumentNode.SelectSingleNode("//p[@class='pagination-info']").InnerHtml)[2].ToString();
                        NoResults.TryAdd(u, Convert.ToInt16(tmp));
                        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@id='aspect_discovery_SimpleSearch_div_search-results']/div");
                        foreach (HtmlNode node in nodes)
                        {
                            try
                            {
                                HtmlNode title = node.SelectSingleNode(".//h4[@class='discoUch']");
                                HtmlNode url = title.SelectSingleNode(".//span");
                                url.ParentNode.RemoveChild(url, true);
                                string type = node.SelectSingleNode(".//div/div/span[@class='tipo_obra']").InnerHtml.ToLower();
                                HtmlNodeCollection authors = node.SelectNodes(".//div[@class='artifact-info']/span[@class='author']/span[@class='ds-dc_contributor_author-authority']");
                                List<Author> autores = new List<Author>();
                                foreach (HtmlNode author in authors)
                                {
                                    string[] nn = StringManipulations.HtmlToPlainText(author.InnerHtml).Split(',');
                                    autores.Add(new Author() { Last = nn[0], Name = nn.Count() > 1 ? nn[1] : "" });
                                }
                                string[] formats = { "yyyy", "yyyy-MM", "yyyy-MM-dd" };
                                DateTime.TryParseExact(node.SelectSingleNode(".//div[@class='artifact-info']/span[@class='publisher-date']/span[@class='date']").InnerHtml,
                                                        formats,
                                                        CultureInfo.InvariantCulture,
                                                        DateTimeStyles.None,
                                                        out DateTime Date);
                                var titls = url.Attributes["title"].Value;
                                List<int> indexes = titls.AllIndexesOf("rft_id");
                                Publication pub = new Publication()
                                {
                                    Source = u,
                                    Title = StringManipulations.HtmlToPlainText(title.InnerHtml),
                                    Uri = new Uri(new Uri(rep), node.SelectSingleNode(".//a").Attributes["href"].Value).ToString(),
                                    Authors = autores,
                                    Company = co,
                                    CompanyId = co.Id,
                                    Date = Date
                                };
                                if (indexes.Count == 3)
                                {
                                    pub.Journal = QueryHelpers.ParseQuery(titls.Substring(indexes[0], indexes[1] - indexes[0]))["rft_id"];
                                    pub.DOI = doi + QueryHelpers.ParseQuery(titls.Substring(indexes[1], indexes[2] - indexes[1]))["rft_id"].ToString().ToLower().Replace("doi: ", "");
                                }
                                if (type.Contains("tesis"))
                                {
                                    pub.Typep = Typep.Tesis;
                                }
                                else if (Regex.IsMatch(type, "art.*culo"))
                                {
                                    pub.Typep = Typep.Articulo;
                                }
                                else
                                {
                                    pub.Typep = Typep.Desconocido;
                                }
                                //pub.CompanyId = pub.Company.Id;
                                try
                                {
                                    pub.Abbr = await nodeServices.InvokeAsync<string>("./wwwroot/js/translate.js", pub.Title, "es");
                                }
                                catch { }
                                Publications.Add(pub);
                            }
                            catch { continue; }
                        }
                    }
                    catch { }
                }
                #endregion
                #region REPOSITORIO ULA
                u = "ula";
                if (src.Contains(u))
                {
                    //15s
                    rep = $"http://medioteca.ulagos.cl/biblioscripts/titulo_claves.idc?texto={q}";
                    var co = _context.Company.SingleOrDefault(c => c.Acronym == u);
                    try
                    {
                        HttpClient hc = new HttpClient();
                        hc.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "windows-1252");
                        byte[] response = await hc.GetByteArrayAsync(rep);
                        string result = Encoding.UTF7.GetString(response, 0, response.Length - 1);
                        HtmlDocument doc = new HtmlDocument() { OptionReadEncoding = true };
                        doc.LoadHtml(result);
                        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//font[@face='Arial']");
                        foreach (HtmlNode node in nodes)
                        {
                            try
                            {
                                var title = node.SelectSingleNode("./a");
                                if(title == null)
                                {
                                    nodes.Remove(node);
                                    continue;
                                }
                                List<Author> autores = new List<Author>();
                                string[] authors = StringManipulations.HtmlToPlainText(node
                                    .SelectSingleNode(".//font/small").InnerHtml).Split(',');
                                autores.Add(new Author() { Last = authors[0], Name = authors[1] });
                                Publication pub = new Publication()
                                {
                                    Typep = Typep.Tesis,
                                    Source = u,
                                    Title = title.InnerHtml.Replace("&iuml;", "'"),
                                    Uri = new Uri(new Uri(rep), title.Attributes["href"].Value).ToString(),
                                    Company = co,
                                    Authors = autores,
                                    CompanyId = co.Id
                                };
                                //pub.CompanyId = pub.Company.Id;
                                try
                                {
                                    pub.Abbr = await nodeServices.InvokeAsync<string>("./wwwroot/js/translate.js", pub.Title, "es");
                                }
                                catch { }
                                Publications.Add(pub);
                            }
                            catch { continue; }
                        }
                        NoResults.TryAdd(u, nodes.Count);
                    }
                    catch { }
                }
                #endregion
                #region REPOSITORIO UTAL
                u = "utal";
                if (src.Contains(u))
                {
                    //10s
                    //srt       rnk         date        popularity      author      title
                    rep = "http://utalca-primo.hosted.exlibrisgroup.com/primo_library/libweb/action/search.do?" +
                    $"fn=search&ct=search&initialSearch=true&mode=Basic&tab=utalca_scope&pag=nxt&indx={(pg - 1) * rpp + 1 - 10}&dum=true&srt={srt_utal}&vid=UTALCA&frbg=&tb=t&vl(freeText0)={q}" +
                    "&scp.scps=scope:(UTALCA),scope:(utalca_aleph),scope:(utalca_dspace),scope:(utalca_cursos),scope:(utalca_ebooks),primo_central_multiple_fe&vl(1147761831UI1)=tesis&vl(1UIStartWith0)=contains&vl(1147761834UI0)=any&vl(1147761834UI0)=title&vl(1147761834UI0)=any";
                    var co = _context.Company.SingleOrDefault(c => c.Acronym == u);

                    try
                    {
                        HttpClient hc = new HttpClient();
                        HttpResponseMessage result = await hc.GetAsync(rep);
                        HtmlDocument doc = new HtmlDocument();
                        doc.Load(await result.Content.ReadAsStreamAsync());
                        string tmp = res.Match(doc.DocumentNode.SelectSingleNode("//em").InnerHtml).ToString().Replace(".", "");
                        NoResults.TryAdd(u, Convert.ToInt16(tmp));
                        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//tr[contains(@class,'EXLResult')]");
                        int len = (rpp > nodes.Count()) ? nodes.Count() : rpp;
                        for (int i = 0; i < len; i++)
                        {
                            try
                            {
                                HtmlNode title = nodes[i].SelectSingleNode(".//h2[@class='EXLResultTitle']/a");
                                List<Author> autores = new List<Author>();
                                Enum.TryParse(nodes[i].SelectSingleNode(".//td[@class='EXLThumbnail']/span[@class='EXLThumbnailCaption']").InnerHtml, out Typep type);
                                string[] authors = StringManipulations.HtmlToPlainText(nodes[i].SelectSingleNode(".//h3[@class='EXLResultAuthor']").InnerHtml).Split(';');
                                foreach (string author in authors)
                                {
                                    string[] nn = author.Split(',');
                                    if (nn.Length > 1)
                                    {
                                        autores.Add(new Author() { Last = nn[0], Name = nn[1] });
                                    }
                                }
                                string journal = nodes[i].SelectSingleNode(".//span[@class='EXLResultDetails']").InnerHtml;
                                string[] formats = { "yyyy", "yyyy-MM", "yyyy-MM-dd" };
                                string year = string.IsNullOrEmpty(journal) ?
                                    Regex.Match(nodes[i].SelectSingleNode(".//h3[@class='EXLResultFourthLine']").InnerHtml, "\\d{4}").Value :
                                    Regex.Match(journal, "[^\\d]\\d{4},").Value.TrimEnd(',').TrimStart();
                                DateTime.TryParseExact(year,
                                                        formats,
                                                        CultureInfo.InvariantCulture,
                                                        DateTimeStyles.None,
                                                        out DateTime Date);
                                Publication pub = new Publication()
                                {
                                    Source = u,
                                    Title = StringManipulations.HtmlToPlainText(title.InnerHtml),
                                    Uri = new Uri(new Uri(rep), title.Attributes["href"].Value).ToString(),
                                    Typep = type,
                                    Authors = autores,
                                    Date = Date,
                                    Company = co,
                                    Journal = journal,
                                    CompanyId = co.Id
                                };
                                //pub.CompanyId = pub.Company.Id;
                                try
                                {
                                    pub.Abbr = await nodeServices.InvokeAsync<string>("./wwwroot/js/translate.js", pub.Title, "es");
                                }
                                catch { }
                                Publications.Add(pub);
                            }
                            catch { continue; }
                        }
                    }
                    catch
                    {
                    }
                }
                #endregion
                #region REPOSITORIO UMAG
                u = "umag";
                if (src.Contains(u))
                {
                    //12s
                    rep = $"http://www.bibliotecadigital.umag.cl/discover?query={q}&rpp={rpp}&page={pg}";
                    var co = _context.Company.SingleOrDefault(c => c.Acronym == u);

                    try
                    {
                        HttpClient hc = new HttpClient();
                        HttpResponseMessage result = await hc.GetAsync(rep);
                        HtmlDocument doc = new HtmlDocument();
                        doc.Load(await result.Content.ReadAsStreamAsync());
                        string tmp = res.Matches(doc.DocumentNode.SelectSingleNode("//h2[@class='lineMid']/span/span").ParentNode.InnerHtml)[1].ToString();
                        NoResults.TryAdd(u, Convert.ToInt16(tmp));
                        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@class='artifact-description']");
                        foreach (HtmlNode node in nodes)
                        {
                            try
                            {
                                List<Author> autores = new List<Author>();
                                HtmlNodeCollection authors = node.SelectNodes(".//div[@class='artifact-info']/span[@class='author']/span");
                                foreach (HtmlNode author in authors)
                                {
                                    string[] nn = StringManipulations.HtmlToPlainText(author.InnerHtml).Split(',');
                                    autores.Add(new Author() { Last = nn[0], Name = nn[1] });
                                }
                                var titl = node.SelectSingleNode(".//div[@class='artifact-title']/a");
                                string[] formats = { "yyyy", "yyyy-MM" };
                                DateTime.TryParseExact(node.SelectSingleNode(".//div[@class='artifact-info']/span[@class='publisher-date']/span[@class='date']").InnerHtml,
                                                        formats,
                                                        CultureInfo.InvariantCulture,
                                                        DateTimeStyles.None,
                                                        out DateTime Date);
                                Publication pub = new Publication
                                {
                                    Source = u,
                                    Title = StringManipulations.HtmlToPlainText(titl.InnerHtml),
                                    Uri = new Uri(new Uri(rep), titl.Attributes["href"].Value).ToString(),
                                    Authors = autores,
                                    Date = Date,
                                    Company = co,
                                    CompanyId = co.Id
                                };
                                //pub.CompanyId = pub.Company.Id;
                                try
                                {
                                    pub.Abbr = await nodeServices.InvokeAsync<string>("./wwwroot/js/translate.js", pub.Title, "es");
                                }
                                catch { }
                                Publications.Add(pub);
                            }
                            catch { continue; }
                        }
                    }
                    catch
                    {
                    }
                }
                #endregion
                #region REPOSITORIO UST
                u = "ust";
                if (src.Contains(u))
                {
                    //13s
                    rep = $"http://www.ust.cl/investigacion/publicaciones/publicaciones-indexadas/page/{pg}/?nombre={q}";
                    var co = _context.Company.SingleOrDefault(c => c.Acronym == u);
                    try
                    {
                        HttpClient hc = new HttpClient();
                        HttpResponseMessage result = await hc.GetAsync(rep);
                        HtmlDocument doc = new HtmlDocument();
                        doc.Load(await result.Content.ReadAsStreamAsync());
                        string tmp = res.Match(doc.DocumentNode.SelectSingleNode("//p[@class='text-small']/strong").InnerHtml).ToString();
                        NoResults.TryAdd(u, Convert.ToInt16(tmp));
                        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@class='content-section white-section articulos-libros']");
                        foreach (HtmlNode node in nodes)
                        {
                            try
                            {
                                List<Author> autores = new List<Author>();
                                HtmlNodeCollection authors = node.SelectNodes(".//p");
                                string[] nn = authors[0].InnerHtml.Split(',');
                                autores.Add(new Author() { Last = Regex.Replace(nn[0], ".*>", ""), Name = nn[1].Trim() });
                                foreach (string author in Regex.Replace(authors[1].InnerHtml.TrimEnd('.'), ".*>", "").Split(","))
                                {
                                    string[] nnn = author.Split(' ');
                                    var autorito = new Author() { Last = nnn[0] };
                                    try
                                    {
                                        autorito.Name = nnn[1];
                                    }
                                    catch { }
                                    autores.Add(autorito);
                                }
                                string[] formats = { "yyyy" };
                                DateTime.TryParseExact(node.SelectNodes(".//div/div/p")[0].InnerHtml,
                                                        formats,
                                                        CultureInfo.InvariantCulture,
                                                        DateTimeStyles.None,
                                                        out DateTime Date);
                                Publication pub = new Publication()
                                {
                                    Source = u,
                                    Title = StringManipulations.HtmlToPlainText(node.SelectSingleNode(".//h3").InnerHtml),
                                    Uri = new Uri(node.SelectSingleNode(".//div/div/p/a").Attributes["href"].Value).ToString(),
                                    Authors = autores,
                                    Date = Date,
                                    Company = co,
                                    CompanyId = co.Id
                                };
                                try
                                {
                                    pub.Abbr = await nodeServices.InvokeAsync<string>("./wwwroot/js/translate.js", pub.Title, "es");
                                }
                                catch { }
                                Publications.Add(pub);
                            }
                            catch { continue; }
                        }
                    }
                    catch
                    {
                    }
                }
                #endregion
                #region REPOSITORIO UCSC
                u = "ucsc";
                if (src.Contains(u))
                {
                    //24s
                    //sort_by   dc.date.issued_dt   dc.title_sort   score
                    //order     asc                 desc
                    rep = $"http://repositoriodigital.ucsc.cl/discover?scope=/&submit=&query={q}&rpp={rpp}&page={pg}&sort_by={sort_by}&order={order}";
                    var co = _context.Company.SingleOrDefault(c => c.Acronym == u);

                    try
                    {
                        //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                        HttpClient hc = new HttpClient();
                        HttpResponseMessage result = await hc.GetAsync(rep);
                        string final = Encoding.UTF8.GetString(await result.Content.ReadAsByteArrayAsync()).Trim().Trim('\0');
                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(final);
                        string tmp = ress.Matches(doc.DocumentNode.SelectSingleNode("//p[@class='pagination-info']").ParentNode.InnerHtml)[2].ToString();
                        NoResults.TryAdd(u, Convert.ToInt16(tmp));
                        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@class='ds-static-div primary']/div/div[contains(@class,'artifact-description')]");
                        foreach (HtmlNode node in nodes)
                        {
                            try
                            {
                                HtmlNode title = node.SelectSingleNode("./a");
                                List<Author> autores = new List<Author>();
                                HtmlNodeCollection authors = node.SelectNodes("./div[@class='artifact-info']/span[@class='author h4']/small/span");
                                foreach (HtmlNode author in authors)
                                {
                                    string[] nn = StringManipulations.HtmlToPlainText(author.InnerHtml).Split(',');
                                    autores.Add(new Author() { Last = nn[0], Name = nn[1] });
                                }
                                string[] formats = { "yyyy", "yyyy-MM" };
                                DateTime.TryParseExact(node.SelectSingleNode("./div[@class='artifact-info']/span[@class='publisher-date h4']/small/span[@class='date']").InnerHtml,
                                                        formats,
                                                        CultureInfo.InvariantCulture,
                                                        DateTimeStyles.None,
                                                        out DateTime Date);
                                Publication pub = new Publication()
                                {
                                    Typep = Typep.Articulo,
                                    Source = u,
                                    Uri = new Uri(new Uri(rep), title.Attributes["href"].Value).ToString(),
                                    Title = StringManipulations.HtmlToPlainText(title.SelectSingleNode("./h4").InnerHtml),
                                    Authors = autores,
                                    Date = Date,
                                    Company = co,
                                    CompanyId = co.Id
                                };
                                try
                                {
                                    pub.Abbr = await nodeServices.InvokeAsync<string>("./wwwroot/js/translate.js", pub.Title, "es");
                                }
                                catch { }
                                Publications.Add(pub);
                            }
                            catch { continue; }
                        }
                    }
                    catch { }
                }
                #endregion
                #region REPOSITORIO UCT
                u = "uct";
                if (src.Contains(u))
                {
                    //31s
                    //sort_by   0(relevance)       1(title)     2(issue date)    3(submit date)
                    //order DESC ASC
                    rep = "http://repositoriodigital.uct.cl/advanced-search?conjunction1=AND&field1=ANY&num_search_field=3&etal=0&rpp=" +
                    $"{rpp}&results_per_page={rpp}&page={pg}&query1={q}&sort_by={srt_uct}&order={order.ToUpper()}";
                    var co = _context.Company.SingleOrDefault(c => c.Acronym == u);

                    try
                    {
                        HttpClient hc = new HttpClient();
                        HttpResponseMessage result = await hc.GetAsync(rep);
                        HtmlDocument doc = new HtmlDocument();
                        doc.Load(await result.Content.ReadAsStreamAsync());
                        string tmp = ress.Match(doc.DocumentNode.SelectSingleNode("//p[@id='aspect_artifactbrowser_AdvancedSearch_p_result-query' and @class='ds-paragraph result-query']").
                            InnerHtml).ToString();
                        NoResults.TryAdd(u, Convert.ToInt16(tmp));
                        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//ul[@class='ds-artifact-list']/li/div[not(@class='artifact-preview')]");
                        foreach (HtmlNode node in nodes)
                        {
                            try
                            {
                                List<Author> autores = new List<Author>();
                                Regex regex = new Regex("[a-zA-Z]");
                                foreach (HtmlNode info in node.SelectNodes(".//div[@class='artifact-info']/text()"))
                                {
                                    if (regex.IsMatch(info.InnerText))
                                    {
                                        string[] nn = info.InnerText.Trim().TrimEnd(';').Split(',');
                                        autores.Add(new Author() { Last = nn[0], Name = nn[1] });
                                    }
                                }
                                string journal = node.SelectSingleNode(".//div[@class='artifact-info']/a[not(@class='enlacerecursivo')]").InnerHtml;
                                string[] formats = { "yyyy" };
                                DateTime.TryParseExact(journal.Substring(journal.LastIndexOf(',') + 2).Remove(4),
                                                        formats,
                                                        CultureInfo.InvariantCulture,
                                                        DateTimeStyles.None,
                                                        out DateTime Date);
                                Publication pub = new Publication
                                {
                                    Source = u,
                                    Title = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(node.SelectSingleNode(".//div[@class='artifact-title']/a/span")
                                        .InnerHtml.ToLower()),
                                    Uri = new Uri(new Uri(rep), node.SelectSingleNode(".//div[@class='artifact-title']/a").Attributes["href"].Value).ToString(),
                                    Journal = journal,
                                    Authors = autores,
                                    Date = Date,
                                    Company = co,
                                    CompanyId = co.Id
                                };
                                //pub.CompanyId = pub.Company.Id;
                                try
                                {
                                    pub.Abbr = await nodeServices.InvokeAsync<string>("./wwwroot/js/translate.js", pub.Title, "es");
                                }
                                catch { }
                                Publications.Add(pub);
                            }
                            catch { continue; }
                        }
                    }
                    catch
                    {
                    }
                }
                #endregion
                #region REPOSITORIO UACH
                u = "uach";
                if (src.Contains(u))
                {
                    //14s
                    //sf        ftitre      fauteur     contributeur        udate       sdxscore
                    rep = "http://cybertesis.uach.cl/sdx/uach/resultats-filtree.xsp?biblio_op=or&figures_op=or&tableaux_op=or&citations_op=or&notes_op=or&base=documents&position=2&texte_op=or&titres=" +
                    $"{q}&tableaux={q}&figures={q}&biblio={q}&notes={q}&citations={q}&texte={q}&hpp={rpp}&p={pg}&sf={srt_uach}";
                    var co = _context.Company.SingleOrDefault(c => c.Acronym == u);

                    try
                    {
                        HttpClient hc = new HttpClient();
                        HttpResponseMessage result = await hc.GetAsync(rep);
                        HtmlDocument doc = new HtmlDocument();
                        doc.Load(await result.Content.ReadAsStreamAsync());
                        string tmp = res.Matches(doc.DocumentNode.SelectSingleNode("//div[@align='left']/b[@class='label']").
                            ParentNode.InnerHtml)[0].ToString();
                        NoResults.TryAdd(u, Convert.ToInt16(tmp));
                        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//td[@valign='top' and @class='ressource' and @align='left']");
                        foreach (HtmlNode node in nodes)
                        {
                            try
                            {
                                List<Author> autores = new List<Author>();
                                HtmlNodeCollection items = node.SelectNodes(".//td[not(@valign='top')]");
                                string[] nn = StringManipulations.HtmlToPlainText(items[1].SelectSingleNode(".//span").InnerHtml).TrimEnd('.').Split(',');
                                autores.Add(new Author() { Last = nn[0], Name = nn[1] });
                                string[] mm = StringManipulations.HtmlToPlainText(items[2].SelectSingleNode(".//span").InnerHtml).TrimEnd('.').Split(',');
                                autores.Add(new Author() { Last = mm[0], Name = mm[1] });
                                string[] formats = { "yyyy", "yyyy-MM" };
                                DateTime.TryParseExact(items[3].SelectSingleNode(".//span").InnerHtml.Remove(4),
                                                        formats,
                                                        CultureInfo.InvariantCulture,
                                                        DateTimeStyles.None,
                                                        out DateTime Date);
                                Publication pub = new Publication
                                {
                                    Source = u,
                                    Title = StringManipulations.HtmlToPlainText(items[0].SelectSingleNode(".//div/a").InnerHtml),
                                    Uri = items[4].SelectSingleNode(".//span/a").InnerHtml,
                                    Authors = autores,
                                    Typep = Typep.Tesis,
                                    Date = Date,
                                    Company = co,
                                    CompanyId = co.Id
                                };
                                //pub.CompanyId = pub.Company.Id;
                                try
                                {
                                    pub.Abbr = await nodeServices.InvokeAsync<string>("./wwwroot/js/translate.js", pub.Title, "es");
                                }
                                catch { }
                                Publications.Add(pub);
                            }
                            catch { continue; }
                        }
                    }
                    catch
                    {
                    }
                }
                #endregion
                #region REPOSITORIO UdeC
                u = "udec";
                if (src.Contains(u))
                {
                    //18s
                    //sort_by   dc.date.issued_dt   dc.title_sort   score
                    //order     asc                 desc
                    rep = "http://repositorio.udec.cl/discover?group_by=none&etal=0&rpp=" +
                    $"{rpp}&page={pg}&query={q}&sort_by={sort_by}&order={order}";
                    var co = _context.Company.SingleOrDefault(c => c.Acronym == u);

                    try
                    {
                        HttpClient hc = new HttpClient();
                        HttpResponseMessage result = await hc.GetAsync(rep);
                        HtmlDocument doc = new HtmlDocument();
                        doc.Load(await result.Content.ReadAsStreamAsync());
                        string tmp = res.Matches(doc.DocumentNode.SelectSingleNode("//h2[@class='ds-div-head']/span").ParentNode.InnerHtml)[1].ToString();
                        NoResults.TryAdd(u, Convert.ToInt16(tmp));
                        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//ul[@class='ds-artifact-list']/ul/li/div[@class='artifact-description']");
                        foreach (HtmlNode node in nodes)
                        {
                            List<Author> autores = new List<Author>();
                            string[] nn = node.SelectSingleNode(".//div[@class='artifact-info']/span[@class='author']/span").InnerHtml.Split(',');
                            autores.Add(new Author() { Last = StringManipulations.HtmlToPlainText(nn[0]), Name = StringManipulations.HtmlToPlainText(nn[1]) });
                            string[] formats = { "yyyy" };
                            DateTime.TryParseExact(node.SelectSingleNode(".//div[@class='artifact-info']/span[@class='publisher-date']/span[@class='date']").InnerHtml,
                                                    formats,
                                                    CultureInfo.InvariantCulture,
                                                    DateTimeStyles.None,
                                                    out DateTime Date);
                            Publication pub = new Publication
                            {
                                Source = u,
                                Title = StringManipulations.HtmlToPlainText(node.SelectSingleNode(".//div[@class='artifact-title']/a").InnerHtml),
                                Uri = new Uri(new Uri(rep), node.SelectSingleNode(".//div[@class='artifact-title']/a").Attributes["href"].Value).ToString(),
                                Authors = autores,
                                Date = Date,
                                Company = co,
                                CompanyId = co.Id
                            };
                            //pub.CompanyId = pub.Company.Id;
                            try
                            {
                                pub.Abbr = await nodeServices.InvokeAsync<string>("./wwwroot/js/translate.js", pub.Title, "es");
                            }
                            catch { }
                            Publications.Add(pub);
                        }
                    }
                    catch
                    {
                    }
                }
                #endregion
                #region REPOSITORIO PUCV
                u = "pucv";
                if (src.Contains(u))
                {
                    //14s
                    rep = $"http://opac.pucv.cl/cgi-bin/wxis.exe/iah/scripts/?IsisScript=iah.xis&lang=es&base=BDTESIS&nextAction=search&exprSearch={q}&isisFrom={(pg - 1) * rpp + 1}";
                    var co = _context.Company.SingleOrDefault(c => c.Acronym == u);

                    try
                    {
                        HttpClient hc = new HttpClient();
                        hc.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");
                        byte[] response = await hc.GetByteArrayAsync(rep);
                        string result = Encoding.UTF7.GetString(response, 0, response.Length - 1);
                        HtmlDocument doc = new HtmlDocument() { OptionReadEncoding = true };
                        doc.LoadHtml(result);
                        string tmp = res.Match(doc.DocumentNode.SelectNodes("//div[@class='rowResult']/div[@class='columnB']")[1].
                            InnerHtml).ToString();
                        NoResults.TryAdd(u, Convert.ToInt16(tmp));
                        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@class='resultCol']");
                        int len = (rpp > nodes.Count()) ? nodes.Count() : rpp;
                        for (int i = 0; i < len; i++)
                        {
                            try
                            {
                                HtmlNodeCollection rows = nodes[i].SelectNodes(".//tr");
                                List<Author> autores = new List<Author>();
                                HtmlNodeCollection authors = rows[1].SelectNodes(".//font/a");
                                foreach (HtmlNode author in authors)
                                {
                                    string[] nn = StringManipulations.HtmlToPlainText(author.InnerHtml).Split(',');
                                    autores.Add(new Author() { Last = nn[0], Name = nn[1] });
                                }
                                string[] formats = { "yyyy", "yyyy-MM" };
                                string date = rows[0].SelectSingleNode(".//font/b/font/font").InnerHtml;
                                date = date.Substring(date.Length - 4);
                                DateTime.TryParseExact(date,
                                                        formats,
                                                        CultureInfo.InvariantCulture,
                                                        DateTimeStyles.None,
                                                        out DateTime Date);
                                Publication pub = new Publication()
                                {
                                    Typep = Typep.Tesis,
                                    Source = u,
                                    Title = StringManipulations.HtmlToPlainText(rows[2].SelectSingleNode(".//font/b").InnerHtml),
                                    Authors = autores,
                                    Date = Date,
                                    Company = co,
                                    CompanyId = co.Id
                                };
                                //pub.CompanyId = pub.Company.Id;
                                try
                                {
                                    pub.Abbr = await nodeServices.InvokeAsync<string>("./wwwroot/js/translate.js", pub.Title, "es");
                                }
                                catch { }
                                Publications.Add(pub);
                            }
                            catch { continue; }
                        }
                    }
                    catch
                    {
                    }
                }
                #endregion
                #region REPOSITORIO PUC
                u = "puc";
                if (src.Contains(u))
                {
                    //15s
                    //sort_by   dc.date.issued_dt   dc.title_sort   score
                    //order     asc                 desc
                    rep = $"https://repositorio.uc.cl/discover?group_by=none&etal=0&rpp={rpp}&page={pg}&query={q}&sort_by={sort_by}&order={order}";
                    var co = _context.Company.SingleOrDefault(c => c.Acronym == u);

                    try
                    {
                        HttpClient hc = new HttpClient();
                        HttpResponseMessage result = await hc.GetAsync(rep);
                        HtmlDocument doc = new HtmlDocument();
                        doc.Load(await result.Content.ReadAsStreamAsync());
                        string tmp = res.Matches(doc.DocumentNode.SelectSingleNode("//h2[@class='ds-div-head']/span").ParentNode.InnerHtml)[1].ToString();
                        NoResults.TryAdd(u, Convert.ToInt16(tmp));
                        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//ul[@class='ds-artifact-list']/ul/li/div[@class='artifact-description']");
                        foreach (HtmlNode node in nodes)
                        {
                            try
                            {
                                HtmlNode title = node.SelectSingleNode(".//div[@class='artifact-title']/a");
                                List<Author> autores = new List<Author>();
                                HtmlNodeCollection authors = node.SelectNodes(".//div[@class='artifact-info']/span[@class='author']/span");
                                foreach (HtmlNode author in authors)
                                {
                                    string[] nn = StringManipulations.HtmlToPlainText(author.InnerHtml.TrimEnd('.')).Split(',');
                                    autores.Add(new Author() { Last = nn[0], Name = nn[1] });
                                }
                                string[] formats = { "yyyy", "yyyy-MM" };
                                DateTime.TryParseExact(node.SelectSingleNode(".//div[@class='artifact-info']/span[@class='publisher-date']/span[@class='date']").InnerHtml.TrimEnd('.'),
                                                        formats,
                                                        CultureInfo.InvariantCulture,
                                                        DateTimeStyles.None,
                                                        out DateTime Date);
                                Publication pub = new Publication()
                                {
                                    Source = u,
                                    Title = StringManipulations.HtmlToPlainText(title.InnerHtml),
                                    Uri = new Uri(new Uri(rep), title.Attributes["href"].Value).ToString(),
                                    Authors = autores,
                                    Date = Date,
                                    Company = co,
                                    CompanyId = co.Id
                                };
                                //pub.CompanyId = pub.Company.Id;
                                try
                                {
                                    pub.Abbr = await nodeServices.InvokeAsync<string>("./wwwroot/js/translate.js", pub.Title, "es");
                                }
                                catch { }
                                Publications.Add(pub);
                            }
                            catch { continue; }
                        }
                    }
                    catch
                    {
                    }
                }
                #endregion
                #region CONICYT
                foreach (string fondo in conicyt.Keys)
                {
                    u = fondo;
                    if (src.Contains(u))
                    {
                        var url = "1080";
                        if (fondo == "FONDECYT")
                        {
                            url += "45";
                        }
                        else if (fondo == "FONDEF")
                        {
                            url += "46";
                        }
                        else if (fondo == "FONDAP")
                        {
                            url += "44";
                        }
                        else if (fondo == "PIA")
                        {
                            url += "42";
                        }
                        else if (fondo == "REGIONAL")
                        {
                            url += "50";
                        }
                        else if (fondo == "BECAS")
                        {
                            url += "40";
                        }
                        else if (fondo == "CONICYT")
                        {
                            url += "88";
                        }
                        else if (fondo == "PROYECTOS")
                        {
                            url = "93475";
                        }
                        rep = $"http://repositorio.conicyt.cl/handle/10533/{url}/discover?query={q}&page={pg - 1}&rpp={rpp}&sort_by={sort_by}&order={order}";
                        var co = _context.Company.SingleOrDefault(c => c.Id == 60915000);
                        try
                        {
                            HttpClient hc = new HttpClient();
                            HttpResponseMessage result = await hc.GetAsync(rep);
                            HtmlDocument doc = new HtmlDocument() { OptionReadEncoding = true };
                            doc.Load(await result.Content.ReadAsStreamAsync());
                            try
                            {
                                string tmp = res.Matches(doc.DocumentNode.SelectSingleNode("//p[@class='pagination-info']").InnerHtml)[2].ToString();
                                NoProjs.TryAdd(u, Convert.ToInt16(tmp));
                            }
                            catch { NoProjs.TryAdd(u, 0); continue; }
                            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@class='col-sm-9 artifact-description']");
                            foreach (HtmlNode node in nodes)
                            {
                                try
                                {
                                    List<Author> autores = new List<Author>();
                                    try
                                    {
                                        string[] authors = node.SelectSingleNode(".//span[@class='ds-dc_contributor_author-authority']").InnerHtml.Split(", ");
                                        foreach (string author in authors)
                                        {
                                            string[] nn = author.Split(' ');
                                            autores.Add(new Author() { Last = nn[0], Name = (nn.Count() > 1) ? nn[1] : "" });
                                        }
                                    }
                                    catch { }
                                    string aall = node.SelectSingleNode(".//span[@class='date']").InnerHtml;
                                    string[] formats = { "yyyy", "yyyy-MM" };
                                    DateTime.TryParseExact(aall,
                                                            formats,
                                                            CultureInfo.InvariantCulture,
                                                            DateTimeStyles.None,
                                                            out DateTime Date);
                                    HtmlNodeCollection items = node.SelectNodes(".//item");
                                    Publication pub = new Publication()
                                    {
                                        Source = u,
                                        Typep = Typep.Proyecto,
                                        Title = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(StringManipulations.HtmlToPlainText(node.SelectSingleNode(".//h4[@class='title-list']").InnerHtml).ToLower()),
                                        Uri = node.SelectSingleNode(".//h4[@class='title-list']").ParentNode.Attributes["href"].Value,
                                        Authors = autores,
                                        Date = Date,
                                        //conicyt
                                        CompanyId = co.Id,
                                        Company = co
                                    };
                                    try
                                    {
                                        pub.Journal = "N° de Proyecto: " + StringManipulations.HtmlToPlainText(items[0].InnerHtml) + " Institución Responsable: " + Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(StringManipulations.HtmlToPlainText(items[3].InnerHtml).ToLower());
                                    }
                                    catch { }
                                    try
                                    {
                                        pub.Abbr = await nodeServices.InvokeAsync<string>("./wwwroot/js/translate.js", pub.Title, "es");
                                    }
                                    catch { }
                                    Publications.Add(pub);
                                }
                                catch { continue; }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                #endregion
                #region FAP
                //u = "FAP";
                //if (src.Contains(u))
                //{
                //    rep = "http://www.fap.cl/controls/neochannels/neo_ch953/neochn953.aspx";

                //    try
                //    {
                //        HttpClient hc = new HttpClient();
                //        HttpResponseMessage result = await hc.GetAsync(rep);
                //        HtmlDocument doc = new HtmlDocument() { OptionReadEncoding = true };
                //        doc.Load(await result.Content.ReadAsStreamAsync());
                //        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//p/a");
                //        NoProjs.TryAdd(u, nodes.Count());
                //        int len = (rpp * pg.Value > nodes.Count()) ? nodes.Count() - 1 : rpp * pg.Value;
                //        int low = rpp * (pg.Value - 1);
                //        if (low <= len)
                //        {
                //            for (int i = low; i < len; i++)
                //            {
                //                try
                //                {
                //                    Publication pub = new Publication()
                //                    {
                //                        Source = u,
                //                        Typep = Typep.Proyecto,
                //                        Uri = new Uri(new Uri(rep), nodes[i].Attributes["href"].Value).ToString(),
                //                        Title = nodes[i].InnerHtml,
                //                        Company = _context.Company.SingleOrDefault(c => c.Acronym == u.ToLower()),
                //                    };
                //                    pub.CompanyId = pub.Company.Id;
                //                    try
                //                    {
                //                        pub.Abbr = await nodeServices.InvokeAsync<string>("./wwwroot/js/translate.js", pub.Title, "es");
                //                    }
                //                    catch { }
                //                    Publications.Add(pub);
                //                }
                //                catch { continue; }
                //            }
                //        }
                //    }
                //    catch
                //    {
                //    }
                //}
                #endregion
                #region SUBPESCA FIPA
                u = "FIPA";
                if (src.Contains(u))
                {
                    rep = $"http://subpesca-engine.newtenberg.com/mod/find/cgi/find.cgi?action=query&engine=SwisheFind&rpp={rpp}&cid=514&stid=&iid=613&grclass=&pnid=&pnid_df=&pnid_tf=&pnid_search=678,682,683,684,681,685,510,522,699,679&limit=200&searchon=&channellink=w3:channel&articlelink=w3:article&pvlink=w3:propertyvalue&notarticlecid=&use_cid_owner_on_links=&show_ancestors=1&show_pnid=1&cids=514&keywords={q}&start={(pg - 1) * rpp}&group=0&expanded=1&searchmode=undefined&prepnidtext=&javascript=1";
                    var co = _context.Company.SingleOrDefault(c => c.Id == 60719000);
                    try
                    {
                        HttpClient hc = new HttpClient();
                        hc.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "windows-1252");
                        byte[] response = await hc.GetByteArrayAsync(rep);
                        string result = Encoding.UTF7.GetString(response, 0, response.Length - 1);
                        HtmlDocument doc = new HtmlDocument() { OptionReadEncoding = true };
                        doc.LoadHtml(result);
                        string tmp = res.Match(doc.DocumentNode.SelectNodes("//p[@class='PP']")[2].InnerHtml).ToString();
                        NoProjs.TryAdd(u, Convert.ToInt16(tmp));
                        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//li/a");
                        foreach (HtmlNode node in nodes)
                        {
                            try
                            {
                                Publication pub = new Publication()
                                {
                                    Source = u,
                                    Typep = Typep.Proyecto,
                                    Uri = new Uri(new Uri("http://www.subpesca.cl/fipa/613/w3-article-88970.html"), node.Attributes["href"].Value).ToString(),
                                    Title = node.InnerHtml,
                                    //subpesca
                                    CompanyId = co.Id,
                                    Company = co
                                };
                                try
                                {
                                    pub.Abbr = await nodeServices.InvokeAsync<string>("./wwwroot/js/translate.js", pub.Title, "es");
                                }
                                catch { }
                                Publications.Add(pub);
                            }
                            catch { continue; }
                        }
                    }
                    catch
                    {
                    }
                }
                #endregion
                #region CORFO
                u = "CORFO";
                if (src.Contains(u))
                {
                    //order     DESC            ASC
                    //sort_by   dc.title_sort
                    //group_by=none
                    rep = "http://repositoriodigital.corfo.cl/discover?query=" +
                    $"{q}&rpp={rpp}&page={pg}&group_by=none&etal=0&sort_by={sort_by.Replace(".issued", "")}&order={order.ToUpper()}";
                    var co = _context.Company.SingleOrDefault(c => c.Id == 60706000);
                    try
                    {
                        HttpClient hc = new HttpClient();
                        HttpResponseMessage result = await hc.GetAsync(rep);
                        HtmlDocument doc = new HtmlDocument() { OptionReadEncoding = true };
                        doc.Load(await result.Content.ReadAsStreamAsync());
                        int pagination = Convert.ToInt16(res.Match(doc.DocumentNode.SelectSingleNode("//p[@class='pagination-info']").InnerHtml).ToString());
                        NoProjs.TryAdd(u, pagination);
                        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'artifact-description')]");
                        foreach (HtmlNode node in nodes)
                        {
                            try
                            {
                                HtmlNode title = node.SelectSingleNode(".//a");
                                var titla = WebUtility.HtmlDecode(title.SelectSingleNode(".//h4/text()").InnerHtml.Trim());
                                string[] formats = { "yyyy", "yyyy-MM", "yyyy-MM-dd" };
                                var data = node.SelectSingleNode(".//span[contains(@class, 'date')]//span").InnerHtml;
                                DateTime.TryParseExact(data,
                                                        formats,
                                                        CultureInfo.InvariantCulture,
                                                        DateTimeStyles.None,
                                                        out DateTime Date);
                                var authors = node.SelectSingleNode(".//span[contains(@class, 'author')]/small").InnerHtml.Split(";");
                                var auts = new List<Author>();
                                foreach (var aut in authors)
                                {
                                    auts.Add(new Author { Name = WebUtility.HtmlDecode(aut) });
                                }
                                var abs = WebUtility.HtmlDecode(node.SelectSingleNode(".//div[contains(@class, 'abstract')]").InnerHtml);
                                Publication pub = new Publication()
                                {
                                    Source = u,
                                    Typep = Typep.Proyecto,
                                    Uri = new Uri(new Uri(rep), title.Attributes["href"].Value).ToString(),
                                    Title = titla,
                                    Date = Date,
                                    //corfo
                                    CompanyId = co.Id,
                                    Company = co,
                                    Authors = auts,
                                    Abstract = abs
                                };
                                try
                                {
                                    pub.Abbr = await nodeServices.InvokeAsync<string>("./wwwroot/js/translate.js", pub.Title, "es");
                                }
                                catch { }
                                Publications.Add(pub);
                            }
                            catch { continue; }
                        }
                    }
                    catch { }
                }
                #endregion
                #region Buscador Google Scholar
                u = "gscholar";
                if (src.Contains(u))
                {
                    rep = $"https://scholar.google.com/scholar?q={q}&start={rpp * (pg - 1) + 1}&scisbd={ggl}";
                    var co = _context.Company.SingleOrDefault(c => c.Id == 55555555);
                    try
                    {
                        HttpClient hc = new HttpClient();
                        hc.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "UTF-8");
                        byte[] response = await hc.GetByteArrayAsync(rep);
                        string result = Encoding.UTF7.GetString(response, 0, response.Length - 1);
                        HtmlDocument doc = new HtmlDocument() { OptionReadEncoding = true };
                        doc.LoadHtml(result);
                        Regex resss = new Regex(@"([0-9]+,)*[0-9]+");
                        Regex yr = new Regex(@"[0-9]{4}");
                        Regex aut = new Regex(@"\A(?:(?![0-9]{4}).)*");
                        string tmp = resss.Match(doc.DocumentNode.SelectSingleNode("//div[@class='gs_ab_mdw']/b").ParentNode.InnerHtml).ToString().Replace(",", "");
                        NoArticles.TryAdd(u, Convert.ToInt32(tmp));
                        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@class='gs_ri']");
                        int len = (rpp > nodes.Count()) ? nodes.Count() : rpp;
                        for (int i = 0; i < len; i++)
                        {
                            try
                            {
                                List<Author> autores = new List<Author>();
                                string ga = StringManipulations.HtmlToPlainText(nodes[i].SelectSingleNode(".//div[@class='gs_a']").InnerHtml);
                                string date = yr.Match(ga).ToString();
                                autores.Add(new Author() { Last = aut.Match(ga).ToString().Trim().Trim('-') });
                                string[] formats = { "yyyy", "yyyy-MM" };
                                DateTime.TryParseExact(date,
                                                        formats,
                                                        CultureInfo.InvariantCulture,
                                                        DateTimeStyles.None,
                                                        out DateTime Date);
                                string title = StringManipulations.HtmlToPlainText(nodes[i].SelectSingleNode(".//h3[@class='gs_rt']").InnerHtml);
                                Publication pub = new Publication()
                                {
                                    Source = u,
                                    Title = title.Substring(title.LastIndexOf(']') + 1),
                                    Uri = nodes[i].SelectSingleNode(".//a").Attributes["href"].Value,
                                    Typep = Typep.Articulo,
                                    Date = Date,
                                    Authors = autores,
                                    CompanyId = co.Id,
                                    Company = co
                                };
                                //pub.CompanyId = pub.Company.Id;
                                try
                                {
                                    pub.Abbr = await nodeServices.InvokeAsync<string>("./wwwroot/js/translate.js", pub.Title, "es");
                                }
                                catch { }
                                Publications.Add(pub);
                            }
                            catch { continue; }
                        }
                    }
                    catch
                    {
                    }
                }
                #endregion
                #region Google Patents
                u = "gpatents";
                if (src.Contains(u))
                {
                    rep = $"https://scholar.google.cl/scholar?as_q={q}" +
                    "&as_epq=&as_oq=&as_eq=&as_occt=any&as_sauthors=&as_publication=Google+Patents&as_ylo=&as_yhi=&btnG=&hl=en&as_sdt=0%2C5&as_vis=1" +
                    $"&start={rpp * (pg - 1) + 1}&scisbd={ggl}";
                    var co = _context.Company.SingleOrDefault(c => c.Id == 55555555);
                    try
                    {
                        HttpClient hc = new HttpClient();
                        hc.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "UTF-8");
                        byte[] response = await hc.GetByteArrayAsync(rep);
                        string result = Encoding.UTF7.GetString(response, 0, response.Length - 1);
                        HtmlDocument doc = new HtmlDocument() { OptionReadEncoding = true };
                        doc.LoadHtml(result);
                        Regex resss = new Regex(@"([0-9]+,)*[0-9]+");
                        Regex yr = new Regex(@"[0-9]{4}");
                        Regex aut = new Regex(@"\A(?:(?![0-9]{4}).)*");
                        string tmp = resss.Match(doc.DocumentNode.SelectSingleNode("//div[@class='gs_ab_mdw']/b").ParentNode.InnerHtml).ToString().Replace(",", "");
                        NoPatents.TryAdd(u, Convert.ToInt32(tmp));
                        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@class='gs_ri']");
                        int len = (rpp > nodes.Count()) ? nodes.Count() : rpp;
                        for (int i = 0; i < len; i++)
                        {
                            try
                            {
                                List<Author> autores = new List<Author>();
                                string ga = StringManipulations.HtmlToPlainText(nodes[i].SelectSingleNode(".//div[@class='gs_a']").InnerHtml);
                                string date = yr.Match(ga).ToString();
                                autores.Add(new Author() { Last = aut.Match(ga).ToString().Trim().Trim('-') });
                                string[] formats = { "yyyy", "yyyy-MM" };
                                DateTime.TryParseExact(date,
                                                        formats,
                                                        CultureInfo.InvariantCulture,
                                                        DateTimeStyles.None,
                                                        out DateTime Date);
                                string title = StringManipulations.HtmlToPlainText(nodes[i].SelectSingleNode(".//h3[@class='gs_rt']").InnerHtml);
                                Publication pub = new Publication()
                                {
                                    Source = u,
                                    Typep = Typep.Patente,
                                    Title = title.Substring(title.LastIndexOf(']') + 1),
                                    Uri = nodes[i].SelectSingleNode(".//a").Attributes["href"].Value,
                                    Date = Date,
                                    Authors = autores,
                                    CompanyId = co.Id,
                                    Company = co
                                };
                                //pub.CompanyId = pub.Company.Id;
                                try
                                {
                                    pub.Abbr = await nodeServices.InvokeAsync<string>("./wwwroot/js/translate.js", pub.Title, "es");
                                }
                                catch { }
                                Publications.Add(pub);
                            }
                            catch { continue; }
                        }
                    }
                    catch
                    {
                    }
                }
                #endregion
                //https://ion.inapi.cl/Patente/ConsultaAvanzadaPatentes.aspx
                //http://www.inapi.cl/dominiopublico/

                var nor = NoResults.Count();
                var nop = NoProjs.Count();
                var noa = NoArticles.Count();
                var nopat = NoPatents.Count();
                var tot = src.Count();
                var NoTot = NoResults.Concat(NoProjs).Concat(NoArticles).Concat(NoPatents)
                .GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);

                int tesiscnt = 0, projcnt = 0, artscnt = 0, patscnt = 0, low1, NoPages;
                var colorTesis = "#5cb85c";
                var colorProyectos = "#d9534f";
                var colorArticulos = "#f0ad4e";
                var colorPatentes = "#5bc0de";

                List<string> tesisList = new List<string> { }, projList = new List<string> { },
                    artsList = new List<string> { }, patsList = new List<string> { };
                string tesisData = "", projData = "", artsData = "", patsData = "";
                var l = new List<int>() { nor, nop, noa, nopat };
                List<string> globalData = new List<string> { };
                if (nor > 0)
                {
                    foreach (var n in NoResults)
                    {
                        var full = ues[n.Key];
                        tesisList.Add("{'repositorio':'" + ues[n.Key].Replace("Universidad", "U.").Replace("Católica", "C.") + " (" + n.Key + ")','resultados':" + n.Value);
                        tesiscnt += n.Value;
                    }
                    if (tesisList.Count() > 0)
                    {
                        tesisData = string.Join(",'color':'" + colorTesis + "'}" + ",", tesisList) + ",'color':'" + colorTesis + "'}";
                        globalData.Add(tesisData);
                    }
                }
                if (nop > 0)
                {
                    foreach (var n in NoProjs)
                    {
                        var full = proj[n.Key];
                        projList.Add("{'repositorio':'" + n.Key + "','resultados':" + n.Value);
                        projcnt += n.Value;
                    }
                    if (projList.Count() > 0)
                    {
                        projData = string.Join(",'color':'" + colorProyectos + "'}" + ",", projList) + ",'color':'" + colorProyectos + "'}";
                        globalData.Add(projData);
                    }
                }
                if (noa > 0)
                {
                    foreach (var n in NoArticles)
                    {
                        var full = gs[n.Key];
                        artsList.Add("{'repositorio':'" + n.Key + "','resultados':" + n.Value);
                        artscnt += n.Value;
                    }
                    if (artsList.Count() > 0)
                    {
                        artsData = string.Join(",'color':'" + colorArticulos + "'}" + ",", artsList) + ",'color':'" + colorArticulos + "'}";
                        globalData.Add(artsData);
                    }
                }
                if (nopat > 0)
                {
                    foreach (var n in NoPatents)
                    {
                        var full = gp[n.Key];
                        patsList.Add("{'repositorio':'" + n.Key + "','resultados':" + n.Value);
                        patscnt += n.Value;
                    }
                    if (patsList.Count() > 0)
                    {
                        patsData = string.Join(",'color':'" + colorPatentes + "'}" + ",", patsList) + ",'color':'" + colorPatentes + "'}";
                        globalData.Add(patsData);
                    }
                }

                ViewData["NoPages"] = NoPages = NoTot.Any() ? (int)Math.Ceiling((double)NoTot.Aggregate((b, r) => b.Value > r.Value ? b : r).Value / rpp) : 1;

                ViewData["any"] = tot > 0;
                ViewData["multiple"] = tot > l.Max();
                ViewData["tesis"] = tesiscnt > 0;
                ViewData[nameof(tot)] = tot;
                ViewData["projects"] = projcnt > 0;
                ViewData["articles"] = artscnt > 0;
                ViewData["patents"] = patscnt > 0;
                ViewData["couple"] = tot > 1;
                var sum = projcnt + tesiscnt + artscnt + patscnt;
                ViewData["all"] = string.Format("{0:n0}", sum);
                ViewData[nameof(artscnt)] = string.Format("{0:n0}", artscnt);
                ViewData[nameof(tesiscnt)] = string.Format("{0:n0}", tesiscnt);
                ViewData[nameof(projcnt)] = string.Format("{0:n0}", projcnt);
                ViewData[nameof(patscnt)] = string.Format("{0:n0}", patscnt);
                ViewData["%arts"] = sum == 0 ? sum : artscnt * 100 / sum;
                ViewData["%tesis"] = sum == 0 ? sum : tesiscnt * 100 / sum;
                ViewData["%proj"] = sum == 0 ? sum : projcnt * 100 / sum;
                ViewData["%pats"] = sum == 0 ? sum : patscnt * 100 / sum;
                ViewData[nameof(patsData)] = patsData;
                ViewData[nameof(artsData)] = artsData;
                ViewData[nameof(tesisData)] = tesisData;
                ViewData[nameof(projData)] = projData;
                ViewData[nameof(globalData)] = string.Join(",", globalData);
                ViewData["arrow"] = asc.Value ? "&#x25BC;" : "&#x25B2;";
                ViewData["prevDisabled"] = pg == 1 ? "disabled" : "";
                ViewData["nextDisabled"] = pg == NoPages ? "disabled" : "";
                ViewData["low"] = low1 = pg.Value > 6 ? pg.Value - 5 : 1;
                ViewData["high"] = NoPages > low1 + 10 ? low1 + 10 : NoPages;

                switch (srt)
                {
                    case "date":
                        publications = asc.Value ?
                            Publications.OrderBy(p => p.Date.Year) :
                            Publications.OrderByDescending(p => p.Date.Year);
                        break;
                    case "title":
                        publications = asc.Value ?
                            Publications.OrderBy(p => p.Title) :
                            Publications.OrderByDescending(p => p.Title);
                        break;
                    default:
                        publications = asc.Value ?
                            Publications.OrderBy(p => p.Source) :
                            Publications.OrderByDescending(p => p.Source);
                        break;
                }
            }

            stopWatch.Stop();
            ViewData["runtime"] = stopWatch.ElapsedMilliseconds;
            ViewData["interval"] = Convert.ToInt32(stopWatch.ElapsedMilliseconds / 500);
            return View(publications);
        }

        public async Task<IActionResult> Translate([FromServices] INodeServices nodeServices)
        {
            var result = await nodeServices.InvokeAsync<string>("./wwwroot/js/translate.js", "hello", "es");
            ViewData["ResultFromNode"] = $"{result}";
            return View();
        }
    }

}
