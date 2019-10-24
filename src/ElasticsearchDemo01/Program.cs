using Nest;
using System;

namespace ElasticsearchDemo01
{
    [ElasticsearchType(IdProperty = "No")]
    public class ogrenci
    {
        public int No { get; set; }

        public string AdiSoyadi { get; set; }

        public int Notu { get; set; }

    }
    class Program
    {
        static void Main(string[] args)
        {

            //önce bağlantı bilgilerini ayarlıyoruz

            var connection = new ConnectionSettings(new Uri("http://localhost:9200"));
            connection.DefaultMappingFor<ogrenci>(ogr => ogr.IndexName("ogrenci")); //index adları küçük olmak zorunda

            var esClient = new ElasticClient(connection);

            //önce index varmı diye kontrol edelim ve kayıt oluşturalım

            if (esClient.Indices.Exists("ogrenci").Exists == false)
            {
                OgrenciOlustur(esClient);
            }

            //notu 22 olan öğrencileri bulalım.
            
            Notu22OlanlariBul(esClient);

            //notu yüksekten en aza olarak sıralayalım.


            var siralaNot = esClient.Search<ogrenci>(sr => sr.MatchAll().Sort(sort => sort.Field("notu",SortOrder.Descending)));
            if (siralaNot.IsValid)
            {
                foreach (var item in siralaNot.Documents)
                {
                    Console.WriteLine($"{item.AdiSoyadi} Not:{item.Notu}");
                }
            }


            Console.ReadKey();


        }

        private static void Notu22OlanlariBul(ElasticClient esClient)
        {
            var response = esClient.Search<ogrenci>(sRequest =>
            {
                sRequest.Query(ct => ct.Term(term => term.Field("notu").Value("22")));

                return sRequest;
            });


            if (response.IsValid)
            {
                Console.WriteLine($"Toplam bulunan öğrenci {response.Total}");

                foreach (var item in response.Documents)
                {
                    Console.WriteLine($"{item.AdiSoyadi} Notu : {item.Notu}");
                }
            }
        }

        private static void OgrenciOlustur(ElasticClient esClient)
        {
            var response = esClient.Indices.Create("ogrenci",
                                 index =>
                                     index.Map<ogrenci>(
                                         x => x.AutoMap()
                                     )
                                 );
            Console.WriteLine("index oluştura başarılı");


            //rast gele kayıt oluşturalım

            for (int i = 1; i <= 10; i++)
            {
                ogrenci ogrenci = new ogrenci();
                ogrenci.AdiSoyadi = $"{i} ogrenci";
                ogrenci.No = i;
                ogrenci.Notu = new Random().Next(1, 100);


                var result = esClient.CreateDocument<ogrenci>(ogrenci);
                if (result.IsValid)
                    Console.WriteLine("kayıt başarılı");
                else
                    Console.WriteLine("kayıt başarısız");


            }

            Console.WriteLine("Öğrenci kayıtları aktarıldı");
        }
    }
}
