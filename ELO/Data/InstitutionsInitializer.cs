using ELO.Data;
using System.Linq;

namespace ELO.Models
{
    public class InstitutionsInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();
            if (!context.Company.Any(c => c.Acronym != null))
            {
                #region Instituciones 33
                var universities = new Company[]
                {
                    new Company {Id=71294800, BsnssName="Centro de Información de Recursos Naturales", Acronym="ciren"},
                    new Company {Id=55555555, BsnssName="Extranjeros y/o sin RUT", Acronym="internacional"},
                    new Company {Id=60605000, BsnssName="Instituto Antártico Chileno", Acronym="inach"},
                    new Company {Id=60706000, BsnssName="Corporación de Fomento de la Producción", Acronym="corfo"},
                    new Company {Id=60719000, BsnssName="Subsecretaría de Pesca y Acuicultura", Acronym="subpesca"},
                    new Company {Id=60910000, BsnssName="Universidad de Chile", Acronym="uchile",Address="Las Palmeras 3425"},
                    new Company {Id=60911000, BsnssName="Universidad de Santiago de Chile", Acronym="usach",Address="Av. Lbertador O Higgins 3363"},
                    new Company {Id=60911006, BsnssName="Universidad del Bío-Bío", Acronym="ubb"},
                    new Company {Id=60915000, BsnssName="Comisión Nacional de Investigación Científica y Tecnológica", Acronym="conicyt"},
                    new Company {Id=60921000, BsnssName="Universidad de Valparaíso", Acronym="uv"},
                    new Company {Id=61310000, BsnssName="Instituto de Fomento Pesquero", Acronym="ifop",Address="J. Domingo Cañas 2277"},
                    new Company {Id=65033643, BsnssName="Centro de Genómica Nutricional y Agroacuícola", Acronym="cgna"},
                    new Company {Id=65091146, BsnssName="Corporación Regional Aysen de Investigación y Desarrollo Cooperativo Centro de Investigación en Ecosistema de la Patagónia ", Acronym="ciep"},
                    new Company {Id=65343160, BsnssName="Asociación Gremial de Mitilicultores de Chile A.G.", Acronym="amichile"},
                    new Company {Id=70300000, BsnssName="Fundación Chile", Acronym="fch"},
                    new Company {Id=70770800, BsnssName="Universidad de Tarapacá", Acronym="uta",Address="Av. General Velasquez 1775"},
                    new Company {Id=70772100, BsnssName="Universidad de Los Lagos", Acronym="ula",Address="Av. Fuchslocher 1305"},
                    new Company {Id=70777500, BsnssName="Universidad Arturo Prat", Acronym="unap"},
                    new Company {Id=70783100, BsnssName="Universidad de La Serena", Acronym="uls"},
                    new Company {Id=70791800, BsnssName="Universidad de Antofagasta", Acronym="ua"},
                    new Company {Id=70885500, BsnssName="Universidad de Talca", Acronym="utal"},
                    new Company {Id=70990700, BsnssName="Universidad Diego Portales", Acronym="udp"},
                    new Company {Id=71133700, BsnssName="Universidad de Magallanes", Acronym="umag",Address="Av. Bulnes 01855"},
                    new Company {Id=71179800, BsnssName="Centro de Estudios Científicos", Acronym="cecs"},
                    new Company {Id=71236700, BsnssName="Universidad de Atacama", Acronym="uda"},
                    new Company {Id=71500500, BsnssName="Universidad Mayor", Acronym="umayor",Address="Camino La Piramide 5750"},
                    new Company {Id=71540100, BsnssName="Universidad Andrés Bello", Acronym="unab"},
                    new Company {Id=71543200, BsnssName="Universidad Adolfo Ibáñez", Acronym="uai"},
                    new Company {Id=71551500, BsnssName="Universidad Santo Tomás", Acronym="ust"},
                    new Company {Id=71554600, BsnssName="Fundación Chinquihue", Acronym="fc",Address="Camino Chinquihue Km 12"},
                    new Company {Id=71614000, BsnssName="Universidad de Los Andes", Acronym="uandes"},
                    new Company {Id=71631900, BsnssName="Universidad San Sebastián", Acronym="uss"},
                    new Company {Id=71633300, BsnssName="Universidad Autónoma de Chile", Acronym="uac"},
                    new Company {Id=71644300, BsnssName="Universidad del Desarrollo", Acronym="udd"},
                    new Company {Id=71915800, BsnssName="Universidad Católica de la Santísima Concepción", Acronym="ucsc"},
                    new Company {Id=71918700, BsnssName="Universidad Católica de Temuco", Acronym="uct",Address="Av. Manuel Montt 56"},
                    new Company {Id=72235100, BsnssName="Gobierno Regional de Valparaíso", Acronym="gorev"},
                    new Company {Id=73124100, BsnssName="Fundación Ciencia para la Vida", Acronym="fcv",Address="Av. Zañartu 1482"},
                    new Company {Id=73923400, BsnssName="Universidad Alberto Hurtado", Acronym="uah"},
                    new Company {Id=81380500, BsnssName="Universidad Austral de Chile", Acronym="uach",Address="Independencia 641"},
                    new Company {Id=81494400, BsnssName="Universidad de Concepción", Acronym="udec"},
                    new Company {Id=81518400, BsnssName="Universidad Católica del Norte", Acronym="ucn"},
                    new Company {Id=81668700, BsnssName="Universidad Técnica Federico Santa María", Acronym="usm"},
                    new Company {Id=81669200, BsnssName="Pontificia Universidad Católica de Valparaíso", Acronym="pucv"},
                    new Company {Id=81698900, BsnssName="Pontificia Universidad Católica de Chile", Acronym="puc"},
                    new Company {Id=82174900, BsnssName="Servicio de Cooperación Técnica", Acronym="sercotec"},
                    new Company {Id=87912900, BsnssName="Universidad de La Frontera", Acronym="ufro"},
                    new Company {Id=96555810, BsnssName="Instituto de Investigación Pesquera", Acronym="inpesca"}
                };
                foreach (Company c in universities)
                {
                    context.Company.Add(c);
                }
                context.SaveChanges();
                #endregion
            }
        }
    }
}
