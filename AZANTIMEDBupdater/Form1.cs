using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MySql.Data.MySqlClient;
using System.Linq;


namespace AZANTIMEDBupdater
{
    public partial class Form1 : Form
    {
        private static IMqttClient client;
        private static IMqttClientOptions options;
        string messageSubuh;
        string message10minSubuh;
        string message15minSubuh;
        string message5minSubuh;

        string messageMaghrib;
        string message5minMaghrib;
        string message10minMaghrib;
        string message15minMaghrib;

        string messageIsyak;
        string message10minIsyak;
        string message5minIsyak;
        string message15minIsyak;

        string messageAsar;
        string message10minAsar;
        string message15minAsar;
        string message5minAsar;

        string message10minZohor;
        string message15minZohor;
        string message5minZohor;
        string messageZohor;

        string connetionString;
        MySqlConnection cnn;
        MySqlCommand command;
        MySqlDataReader dataReader;
        string server = "localhost";
        string database = "dbo";
        string uid = "root";
        string password = "1234";

        delegate void SetTextCallback(Label lb);
        GetTImeGMT getGmt = new GetTImeGMT();
        public DateTime GetGMTTime;

        private int hourelapse = 0;
        private int minuteelapse = 0;
        private int secondelapse = 0;
        bool IsTimeRunning = false;
        string timeGMT7;
        string timeGMT8;
        string timeGMT9;
        string[] globalDateTime;
        string[] convertGMT7;
        string[] convertGMT9;
        string dateGMT7;
        string dateGMT9;
        public string date;

        Dictionary<int, List<Location>> locations8 = new Dictionary<int, List<Location>>();
        Dictionary<int, List<Location>> locations7 = new Dictionary<int, List<Location>>();
        Dictionary<int, List<Location>> locations9 = new Dictionary<int, List<Location>>();

        Dictionary<int, List<AzanTimeMethod>> azanTimeMethods8 = new Dictionary<int, List<AzanTimeMethod>>();
        Dictionary<int, List<AzanTimeMethod>> azanTimeMethods7 = new Dictionary<int, List<AzanTimeMethod>>();
        Dictionary<int, List<AzanTimeMethod>> azanTimeMethods9 = new Dictionary<int, List<AzanTimeMethod>>();

        Dictionary<int, List<AzanTimeMethod15mins>> timeMinus15min8 = new Dictionary<int, List<AzanTimeMethod15mins>>();
        Dictionary<int, List<AzanTimeMethod15mins>> timeMinus15min7 = new Dictionary<int, List<AzanTimeMethod15mins>>();
        Dictionary<int, List<AzanTimeMethod15mins>> timeMinus15min9 = new Dictionary<int, List<AzanTimeMethod15mins>>();

        Dictionary<int, List<AzanTimeMethod10mins>> timeMinus10min8 = new Dictionary<int, List<AzanTimeMethod10mins>>();
        Dictionary<int, List<AzanTimeMethod10mins>> timeMinus10min7 = new Dictionary<int, List<AzanTimeMethod10mins>>();
        Dictionary<int, List<AzanTimeMethod10mins>> timeMinus10min9 = new Dictionary<int, List<AzanTimeMethod10mins>>();

        Dictionary<int, List<AzanTimeMethod5min>> timeMinus5min8 = new Dictionary<int, List<AzanTimeMethod5min>>();
        Dictionary<int, List<AzanTimeMethod5min>> timeMinus5min7 = new Dictionary<int, List<AzanTimeMethod5min>>();
        Dictionary<int, List<AzanTimeMethod5min>> timeMinus5min9 = new Dictionary<int, List<AzanTimeMethod5min>>();

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            IsTimeRunning = true;
            Thread process = new Thread(starttimer); process.Start();
            Thread process2 = new Thread(counter); process2.Start();         
        }
        private void SetText(Label lb)
        {
            try
            {
                if (lb.InvokeRequired)
                {
                    SetTextCallback d = new SetTextCallback(SetText);
                    this.Invoke(d, new object[] { lb });
                }
                else
                {
                    lb.Text = System.DateTime.Now.ToString("yyyy MM dd" + "," + "h:mm tt");
                    GetGMTTime = Convert.ToDateTime(lb.Text);

                    convertGMT7 = getGmt.IndoWestGMT7(GetGMTTime).ToString("yyyy MM dd" + "," + "h:mm tt").Split(',');
                    convertGMT9 = getGmt.indoeastGMT9(GetGMTTime).ToString("yyyy MM dd" + "," + "h:mm tt").Split(',');
                    globalDateTime = lb.Text.Split(',');

                    date = globalDateTime[0];
                    dateGMT7 = convertGMT7[0];
                    dateGMT9 = convertGMT9[0];

                    timeGMT8 = globalDateTime[1];
                    timeGMT7 = convertGMT7[1];
                    timeGMT9 = convertGMT9[1];

                    DateOnly3.Text = dateGMT9;
                    DateOnly2.Text = dateGMT7;
                    DateOnly.Text = date;

                    TimeGMT8.Text = timeGMT8;
                    TimeGMT7.Text = timeGMT7;
                    TimeGMT9.Text = timeGMT9;

                    secondlapse.Text = secondelapse.ToString();
                    Minute.Text = minuteelapse.ToString();
                    Hour.Text = hourelapse.ToString();
                }
                refresh_Dictionary();
                Creating_ThreeTimeZoneDictionary();
                AddingDateTo_DicTionary();

                Task.Run(async () => {

                await Task.Delay(1000);
                await setReminder15minDictionary();
                await  setReminder10minDictionary();
                await setReminder5minDictionary();

                await Execute_MQtt_15minReminderZone7();
                await Execute_MQtt_10minReminderZone7();
                await Execute_MQtt_5minReminderZone7();
                await Execute_MQtt_Zone7();

                await Execute_MQtt_15minReminderZone8();
                await Execute_MQtt_10minReminderZone8();
                await Execute_MQtt_5minReminderZone8();
                await Execute_MQtt_Zone8();

                await Execute_MQtt_15minReminderZone9();
                await Execute_MQtt_10minReminderZone9();
                await Execute_MQtt_5minReminderZone9();
                await Execute_MQtt_Zone9();
            });
            }
            catch (Exception e){ Console.WriteLine(e.ToString());}
        }
        private void starttimer()
        {
            while (IsTimeRunning == true)
            SetText(fulldateTime);
            Thread.Sleep(500);
        }
        private void counter()
        {
            while (IsTimeRunning == true)
            {
                secondelapse++;
                if (secondelapse == 60)
                {
                    secondelapse = 0;
                    minuteelapse++;
                    if (minuteelapse == 60)
                    {
                        minuteelapse = 0;
                        hourelapse++;
                    }
                }
                Thread.Sleep(1000);
            }
        }
        private void refresh_Dictionary()
        {
            foreach (var date7 in azanTimeMethods7.Values.ToList())
            {
                if (date7[0].TarikhMiladi != dateGMT7)
                {
                    azanTimeMethods7.Clear();
                    locations7.Clear();
                    timeMinus10min7.Clear();
                    timeMinus15min7.Clear();
                    timeMinus5min7.Clear();
                    listBox7.Items.Clear();
                }
            }
            foreach (var date8 in azanTimeMethods8.Values.ToList())
            {
                if (date8[0].TarikhMiladi != date)
                {
                    azanTimeMethods8.Clear();
                    locations8.Clear();
                    timeMinus10min8.Clear();
                    timeMinus15min8.Clear();
                    timeMinus5min8.Clear();
                    listBox8.Items.Clear();
                }
            }
            foreach (var date9 in azanTimeMethods9.Values.ToList())
            {
                if (date9[0].TarikhMiladi != dateGMT9)
                {
                    azanTimeMethods9.Clear();
                    locations9.Clear();
                    timeMinus10min9.Clear();
                    timeMinus15min9.Clear();
                    timeMinus5min9.Clear();
                    listBox9.Items.Clear(); 
                }
            }
        }
        private void Creating_ThreeTimeZoneDictionary()
        {
            Location zone2_GMT8 = new Location(2, "Amlapura");
            Location zone3_GMT8 = new Location(3, "Amuntai");
            Location zone8_GMT8 = new Location(8, "Bajawa");
            Location zone17_GMT8 = new Location(17, "Bangli");
            Location zone20_GMT8 = new Location(20, "Banjarmasin");
            Location zone22_GMT8 = new Location(22, "Bantaeng");
            Location zone26_GMT8 = new Location(26, "Barabai");
            Location zone27_GMT8 = new Location(27, "Barito");
            Location zone28_GMT8 = new Location(28, "Barru");
            Location zone40_GMT8 = new Location(40, "Bima");
            Location zone49_GMT8 = new Location(49, "Bontang");
            Location zone53_GMT8 = new Location(53, "Bulukumba");
            Location zone64_GMT8 = new Location(64, "denpasar");
            Location zone67_GMT8 = new Location(67, "dompu");
            Location zone68_GMT8 = new Location(68, "donggala");
            Location zone70_GMT8 = new Location(70, "ende");
            Location zone72_GMT8 = new Location(72, "enrekang");
            Location zone77_GMT8 = new Location(77, "gorontalo");
            Location zone85_GMT8 = new Location(85, "jeneponto");
            Location zone89_GMT8 = new Location(89, "kalabahi");
            Location zone91_GMT8 = new Location(91, "kandangan");
            Location zone94_GMT8 = new Location(94, "kasungan");
            Location zone99_GMT8 = new Location(99, "kefamenanu");
            Location zone101_GMT8 = new Location(101, "kendari");
            Location zone106_GMT8 = new Location(106, "kolaka");
            Location zone107_GMT8 = new Location(107, "kotabarupulaulaut");
            Location zone110_GMT8 = new Location(110, "kotamobagu");
            Location zone117_GMT8 = new Location(117, "kupang");
            Location zone124_GMT8 = new Location(124, "larantuka");
            Location zone127_GMT8 = new Location(127, "limboto");
            Location zone133_GMT8 = new Location(133, "luwuk");
            Location zone138_GMT8 = new Location(138, "majene");
            Location zone139_GMT8 = new Location(139, "makale");
            Location zone140_GMT8 = new Location(140, "makassar");
            Location zone142_GMT8 = new Location(142, "mamuju");
            Location zone145_GMT8 = new Location(145, "marabahan");
            Location zone146_GMT8 = new Location(146, "maros");
            Location zone147_GMT8 = new Location(147, "martapura");
            Location zone149_GMT8 = new Location(149, "mataram");
            Location zone150_GMT8 = new Location(150, "maumere");
            Location zone153_GMT8 = new Location(153, "menado");
            Location zone166_GMT8 = new Location(166, "negara");
            Location zone169_GMT8 = new Location(169, "nunukan");
            Location zone176_GMT8 = new Location(176, "palangkaraya");
            Location zone177_GMT8 = new Location(177, "palembang");
            Location zone178_GMT8 = new Location(178, "palopo");
            Location zone179_GMT8 = new Location(179, "palu");
            Location zone182_GMT8 = new Location(182, "pangkajene");
            Location zone183_GMT8 = new Location(183, "pangkajenesidenreng");
            Location zone188_GMT8 = new Location(188, "parepare");
            Location zone198_GMT8 = new Location(198, "pinrang");
            Location zone199_GMT8 = new Location(199, "pleihari");
            Location zone200_GMT8 = new Location(200, "polewali");
            Location zone204_GMT8 = new Location(204, "poso");
            Location zone215_GMT8 = new Location(215, "raha");
            Location zone217_GMT8 = new Location(217, "rantau");
            Location zone219_GMT8 = new Location(219, "rantepao");
            Location zone222_GMT8 = new Location(222, "ruteng");
            Location zone231_GMT8 = new Location(231, "selong");
            Location zone233_GMT8 = new Location(233, "sengkang");
            Location zone241_GMT8 = new Location(241, "singaraja");
            Location zone243_GMT8 = new Location(243, "sinjai");
            Location zone248_GMT8 = new Location(248, "soasiu");
            Location zone249_GMT8 = new Location(249, "soe");
            Location zone259_GMT8 = new Location(259, "sumbawabesar");
            Location zone260_GMT8 = new Location(260, "sumedang");
            Location zone264_GMT8 = new Location(264, "sungguminasa");
            Location zone267_GMT8 = new Location(267, "tabanan");
            Location zone268_GMT8 = new Location(268, "tahuna");
            Location zone269_GMT8 = new Location(269, "takalar");
            Location zone271_GMT8 = new Location(271, "tamianglayang");
            Location zone272_GMT8 = new Location(272, "tanahgrogot");
            Location zone281_GMT8 = new Location(281, "tarakan");
            Location zone290_GMT8 = new Location(290, "tolitoli");
            Location zone291_GMT8 = new Location(291, "tondano");
            Location zone298_GMT8 = new Location(298, "waikabubak");
            Location zone300_GMT8 = new Location(300, "wamena");
            Location zone301_GMT8 = new Location(301, "watampone");
            Location zone302_GMT8 = new Location(302, "watansoppeng");
           
            if (!locations8.ContainsKey(zone2_GMT8.Zone))
            {
                locations8.Add(zone2_GMT8.Zone, new List<Location> { zone2_GMT8 });
                locations8.Add(zone3_GMT8.Zone, new List<Location> { zone3_GMT8 });
                locations8.Add(zone8_GMT8.Zone, new List<Location> { zone8_GMT8 });
                locations8.Add(zone17_GMT8.Zone, new List<Location> { zone17_GMT8 });
                locations8.Add(zone20_GMT8.Zone, new List<Location> { zone20_GMT8 });
                locations8.Add(zone22_GMT8.Zone, new List<Location> { zone22_GMT8 });
                locations8.Add(zone26_GMT8.Zone, new List<Location> { zone26_GMT8 });
                locations8.Add(zone27_GMT8.Zone, new List<Location> { zone27_GMT8 });
                locations8.Add(zone28_GMT8.Zone, new List<Location> { zone28_GMT8 });
                locations8.Add(zone40_GMT8.Zone, new List<Location> { zone40_GMT8 });
                locations8.Add(zone49_GMT8.Zone, new List<Location> { zone49_GMT8 });
                locations8.Add(zone53_GMT8.Zone, new List<Location> { zone53_GMT8 });
                locations8.Add(zone64_GMT8.Zone, new List<Location> { zone64_GMT8 });
                locations8.Add(zone67_GMT8.Zone, new List<Location> { zone67_GMT8 });
                locations8.Add(zone68_GMT8.Zone, new List<Location> { zone68_GMT8 });
                locations8.Add(zone70_GMT8.Zone, new List<Location> { zone70_GMT8 });
                locations8.Add(zone72_GMT8.Zone, new List<Location> { zone72_GMT8 });
                locations8.Add(zone77_GMT8.Zone, new List<Location> { zone77_GMT8 });
                locations8.Add(zone85_GMT8.Zone, new List<Location> { zone85_GMT8 });
                locations8.Add(zone89_GMT8.Zone, new List<Location> { zone89_GMT8 });
                locations8.Add(zone91_GMT8.Zone, new List<Location> { zone91_GMT8 });
                locations8.Add(zone94_GMT8.Zone, new List<Location> { zone94_GMT8 });
                locations8.Add(zone99_GMT8.Zone, new List<Location> { zone99_GMT8 });
                locations8.Add(zone101_GMT8.Zone, new List<Location> { zone101_GMT8 });
                locations8.Add(zone106_GMT8.Zone, new List<Location> { zone106_GMT8 });
                locations8.Add(zone107_GMT8.Zone, new List<Location> { zone107_GMT8 });
                locations8.Add(zone117_GMT8.Zone, new List<Location> { zone117_GMT8 });
                locations8.Add(zone110_GMT8.Zone, new List<Location> { zone110_GMT8 });
                locations8.Add(zone124_GMT8.Zone, new List<Location> { zone124_GMT8 });
                locations8.Add(zone127_GMT8.Zone, new List<Location> { zone127_GMT8 });
                locations8.Add(zone133_GMT8.Zone, new List<Location> { zone133_GMT8 });
                locations8.Add(zone138_GMT8.Zone, new List<Location> { zone138_GMT8 });
                locations8.Add(zone139_GMT8.Zone, new List<Location> { zone139_GMT8 });
                locations8.Add(zone140_GMT8.Zone, new List<Location> { zone140_GMT8 });
                locations8.Add(zone142_GMT8.Zone, new List<Location> { zone142_GMT8 });
                locations8.Add(zone145_GMT8.Zone, new List<Location> { zone145_GMT8 });
                locations8.Add(zone146_GMT8.Zone, new List<Location> { zone146_GMT8 });
                locations8.Add(zone147_GMT8.Zone, new List<Location> { zone147_GMT8 });
                locations8.Add(zone149_GMT8.Zone, new List<Location> { zone149_GMT8 });
                locations8.Add(zone150_GMT8.Zone, new List<Location> { zone150_GMT8 });
                locations8.Add(zone153_GMT8.Zone, new List<Location> { zone153_GMT8 });
                locations8.Add(zone166_GMT8.Zone, new List<Location> { zone166_GMT8 });
                locations8.Add(zone169_GMT8.Zone, new List<Location> { zone169_GMT8 });
                locations8.Add(zone176_GMT8.Zone, new List<Location> { zone176_GMT8 });
                locations8.Add(zone177_GMT8.Zone, new List<Location> { zone177_GMT8 });
                locations8.Add(zone178_GMT8.Zone, new List<Location> { zone178_GMT8 });
                locations8.Add(zone179_GMT8.Zone, new List<Location> { zone179_GMT8 });
                locations8.Add(zone182_GMT8.Zone, new List<Location> { zone182_GMT8 });
                locations8.Add(zone183_GMT8.Zone, new List<Location> { zone183_GMT8 });
                locations8.Add(zone188_GMT8.Zone, new List<Location> { zone188_GMT8 });
                locations8.Add(zone198_GMT8.Zone, new List<Location> { zone198_GMT8 });
                locations8.Add(zone199_GMT8.Zone, new List<Location> { zone199_GMT8 });
                locations8.Add(zone200_GMT8.Zone, new List<Location> { zone200_GMT8 });
                locations8.Add(zone204_GMT8.Zone, new List<Location> { zone204_GMT8 });
                locations8.Add(zone215_GMT8.Zone, new List<Location> { zone215_GMT8 });
                locations8.Add(zone217_GMT8.Zone, new List<Location> { zone217_GMT8 });
                locations8.Add(zone219_GMT8.Zone, new List<Location> { zone219_GMT8 });
                locations8.Add(zone222_GMT8.Zone, new List<Location> { zone222_GMT8 });
                locations8.Add(zone231_GMT8.Zone, new List<Location> { zone231_GMT8 });
                locations8.Add(zone233_GMT8.Zone, new List<Location> { zone233_GMT8 });
                locations8.Add(zone241_GMT8.Zone, new List<Location> { zone241_GMT8 });
                locations8.Add(zone248_GMT8.Zone, new List<Location> { zone248_GMT8 });
                locations8.Add(zone249_GMT8.Zone, new List<Location> { zone249_GMT8 });
                locations8.Add(zone259_GMT8.Zone, new List<Location> { zone259_GMT8 });
                locations8.Add(zone260_GMT8.Zone, new List<Location> { zone260_GMT8 });
                locations8.Add(zone264_GMT8.Zone, new List<Location> { zone264_GMT8 });
                locations8.Add(zone267_GMT8.Zone, new List<Location> { zone267_GMT8 });
                locations8.Add(zone268_GMT8.Zone, new List<Location> { zone268_GMT8 });
                locations8.Add(zone243_GMT8.Zone, new List<Location> { zone243_GMT8 });
                locations8.Add(zone269_GMT8.Zone, new List<Location> { zone269_GMT8 });
                locations8.Add(zone271_GMT8.Zone, new List<Location> { zone271_GMT8 });
                locations8.Add(zone272_GMT8.Zone, new List<Location> { zone272_GMT8 });
                locations8.Add(zone281_GMT8.Zone, new List<Location> { zone281_GMT8 });
                locations8.Add(zone290_GMT8.Zone, new List<Location> { zone290_GMT8 });
                locations8.Add(zone291_GMT8.Zone, new List<Location> { zone291_GMT8 });
                locations8.Add(zone298_GMT8.Zone, new List<Location> { zone298_GMT8 });
                locations8.Add(zone300_GMT8.Zone, new List<Location> { zone300_GMT8 });
                locations8.Add(zone301_GMT8.Zone, new List<Location> { zone301_GMT8 });
                locations8.Add(zone302_GMT8.Zone, new List<Location> { zone302_GMT8 });
            }

            Location zone1_GMT7 = new Location(1, "Ambarawa");
            Location zone4_GMT7 = new Location(4, "Argamakmur");
            Location zone7_GMT7 = new Location(7, "bagansiapiapi");
            Location zone9_GMT7 = new Location(9, "Balige");
            Location zone10_GMT7 = new Location(10, "balikpapan");
            Location zone11_GMT7 = new Location(11, "bandaaceh");
            Location zone12_GMT7 = new Location(12, "Bandarlampung");
            Location zone13_GMT7 = new Location(13, "Bandung");
            Location zone14_GMT7 = new Location(14, "Bangkalan");
            Location zone15_GMT7 = new Location(15, "Bangkinang");
            Location zone16_GMT7 = new Location(16, "Bangko");
            Location zone18_GMT7 = new Location(18, "Banjar");
            Location zone19_GMT7 = new Location(19, "banjarbaru");
            Location zone21_GMT7 = new Location(21, "Banjarnegara");
            Location zone23_GMT7 = new Location(23, "Banten");
            Location zone24_GMT7 = new Location(24, "Bantul");
            Location zone25_GMT7 = new Location(25, "Banyuwangi");
            Location zone29_GMT7 = new Location(29, "Batam");
            Location zone30_GMT7 = new Location(30, "Batang");
            Location zone31_GMT7 = new Location(31, "Batu");
            Location zone32_GMT7 = new Location(32, "Baturaja");
            Location zone33_GMT7 = new Location(33, "Batusangkar");
            Location zone35_GMT7 = new Location(35, "Bekasi");
            Location zone36_GMT7 = new Location(36, "Bengkalis");
            Location zone37_GMT7 = new Location(37, "Bengkulu");
            Location zone41_GMT7 = new Location(41, "Binjai");
            Location zone42_GMT7 = new Location(42, "Bireuen");
            Location zone44_GMT7 = new Location(44, "Blitar");
            Location zone45_GMT7 = new Location(45, "Blora");
            Location zone46_GMT7 = new Location(46, "Bogor");
            Location zone47_GMT7 = new Location(47, "Bojonegoro");
            Location zone48_GMT7 = new Location(48, "Bondowoso");
            Location zone50_GMT7 = new Location(50, "Boyolali");
            Location zone51_GMT7 = new Location(51, "Brebes");
            Location zone52_GMT7 = new Location(52, "BukitTinggi");
            Location zone54_GMT7 = new Location(54, "Buntok");
            Location zone55_GMT7 = new Location(55, "Cepu");
            Location zone56_GMT7 = new Location(56, "cianjur");
            Location zone57_GMT7 = new Location(57, "cibinong");
            Location zone58_GMT7 = new Location(58, "cilacap");
            Location zone59_GMT7 = new Location(59, "cilegon");
            Location zone60_GMT7 = new Location(60, "cimahi");
            Location zone61_GMT7 = new Location(61, "cirebon");
            Location zone62_GMT7 = new Location(62, "curup");
            Location zone63_GMT7 = new Location(63, "demak");
            Location zone65_GMT7 = new Location(65, "depok");
            Location zone69_GMT7 = new Location(69, "dumai");
            Location zone71_GMT7 = new Location(71, "enggano");
            Location zone74_GMT7 = new Location(74, "garut");
            Location zone75_GMT7 = new Location(75, "gianyar");
            Location zone76_GMT7 = new Location(76, "gombong");
            Location zone78_GMT7 = new Location(78, "gresik");
            Location zone79_GMT7 = new Location(79, "gunungsitoli");
            Location zone80_GMT7 = new Location(80, "indramayu");
            Location zone81_GMT7 = new Location(81, "jakarta");
            Location zone82_GMT7 = new Location(82, "jambi");
            Location zone84_GMT7 = new Location(84, "jember");
            Location zone86_GMT7 = new Location(86, "jepara");
            Location zone87_GMT7 = new Location(87, "jombang");
            Location zone88_GMT7 = new Location(88, "kabanjahe");
            Location zone90_GMT7 = new Location(90, "kalianda");
            Location zone92_GMT7 = new Location(92, "karanganyar");
            Location zone93_GMT7 = new Location(93, "karawang");
            Location zone95_GMT7 = new Location(95, "kayuagung");
            Location zone96_GMT7 = new Location(96, "kebumen");
            Location zone97_GMT7 = new Location(97, "kediri");
            Location zone100_GMT7 = new Location(100, "kendal");
            Location zone102_GMT7 = new Location(102, "kertosono");
            Location zone103_GMT7 = new Location(103, "ketapang");
            Location zone104_GMT7 = new Location(104, "kisaran");
            Location zone105_GMT7 = new Location(105, "klaten");
            Location zone108_GMT7 = new Location(108, "kotabumi");
            Location zone109_GMT7 = new Location(109, "kotajantho");
            Location zone111_GMT7 = new Location(111, "kualakapuas");
            Location zone112_GMT7 = new Location(112, "kualakurun");
            Location zone113_GMT7 = new Location(113, "kualapembuang");
            Location zone114_GMT7 = new Location(114, "kualatungkal");
            Location zone115_GMT7 = new Location(115, "kudus");
            Location zone116_GMT7 = new Location(116, "kuningan");
            Location zone118_GMT7 = new Location(118, "kutacane");
            Location zone119_GMT7 = new Location(119, "kutoarjo");
            Location zone120_GMT7 = new Location(120, "labuhan");
            Location zone121_GMT7 = new Location(121, "lahat");
            Location zone122_GMT7 = new Location(122, "lamongan");
            Location zone123_GMT7 = new Location(123, "langsa");
            Location zone125_GMT7 = new Location(125, "lawang");
            Location zone126_GMT7 = new Location(126, "lhoseumawe");
            Location zone128_GMT7 = new Location(128, "lubukbasung");
            Location zone129_GMT7 = new Location(129, "lubuklinggau");
            Location zone130_GMT7 = new Location(130, "lubukpakam");
            Location zone131_GMT7 = new Location(131, "lubuksikaping");
            Location zone132_GMT7 = new Location(132, "lumajang");
            Location zone134_GMT7 = new Location(134, "madiun");
            Location zone135_GMT7 = new Location(135, "magelang");
            Location zone136_GMT7 = new Location(136, "magetan");
            Location zone137_GMT7 = new Location(137, "majalengka");
            Location zone141_GMT7 = new Location(141, "malang");
            Location zone143_GMT7 = new Location(143, "manna");
            Location zone151_GMT7 = new Location(151, "medan");
            Location zone152_GMT7 = new Location(152, "mempawah");
            Location zone154_GMT7 = new Location(154, "mentok");
            Location zone156_GMT7 = new Location(156, "metro");
            Location zone157_GMT7 = new Location(157, "meulaboh");
            Location zone158_GMT7 = new Location(158, "mojokerto");
            Location zone159_GMT7 = new Location(159, "muarabulian");
            Location zone160_GMT7 = new Location(160, "muarabungo");
            Location zone161_GMT7 = new Location(161, "muaraenim");
            Location zone162_GMT7 = new Location(162, "muarateweh");
            Location zone163_GMT7 = new Location(163, "muarosijunjung");
            Location zone164_GMT7 = new Location(164, "muntilan");
            Location zone167_GMT7 = new Location(167, "nganjuk");
            Location zone168_GMT7 = new Location(168, "ngawi");
            Location zone170_GMT7 = new Location(170, "pacitan");
            Location zone171_GMT7 = new Location(171, "padang1");
            Location zone172_GMT7 = new Location(172, "padangpanjang");
            Location zone173_GMT7 = new Location(173, "padangsidempuan");
            Location zone174_GMT7 = new Location(174, "pagaralam");
            Location zone175_GMT7 = new Location(175, "painan");
            Location zone180_GMT7 = new Location(180, "pamekasan");
            Location zone181_GMT7 = new Location(181, "pandeglang");
            Location zone184_GMT7 = new Location(184, "pangkalanbun");
            Location zone185_GMT7 = new Location(185, "pangkalpinang");
            Location zone186_GMT7 = new Location(186, "panyabungan");
            Location zone187_GMT7 = new Location(187, "pare");
            Location zone189_GMT7 = new Location(189, "pariaman");
            Location zone190_GMT7 = new Location(190, "pasuruan");
            Location zone191_GMT7 = new Location(191, "pati");
            Location zone192_GMT7 = new Location(192, "payakumbuh");
            Location zone193_GMT7 = new Location(193, "pekalongan");
            Location zone194_GMT7 = new Location(194, "pekanbaru");
            Location zone195_GMT7 = new Location(195, "pemalang");
            Location zone196_GMT7 = new Location(196, "pematangsiantar");
            Location zone197_GMT7 = new Location(197, "pendopo");
            Location zone201_GMT7 = new Location(201, "pondokgede");
            Location zone202_GMT7 = new Location(202, "ponorogo");
            Location zone203_GMT7 = new Location(203, "pontianak");
            Location zone205_GMT7 = new Location(205, "prabumulih");
            Location zone206_GMT7 = new Location(206, "praya");
            Location zone207_GMT7 = new Location(207, "probolinggo");
            Location zone208_GMT7 = new Location(208, "purbalingga");
            Location zone209_GMT7 = new Location(209, "purukcahu");
            Location zone210_GMT7 = new Location(210, "purwakarta");
            Location zone211_GMT7 = new Location(211, "purwodadigrobogan");
            Location zone212_GMT7 = new Location(212, "purwokerto");
            Location zone213_GMT7 = new Location(213, "purworejo");
            Location zone214_GMT7 = new Location(214, "putussibau");
            Location zone216_GMT7 = new Location(216, "rangkasbitung");
            Location zone218_GMT7 = new Location(218, "rantauprapat");
            Location zone220_GMT7 = new Location(220, "rembang");
            Location zone221_GMT7 = new Location(221, "rengat");
            Location zone223_GMT7 = new Location(223, "sabang");
            Location zone224_GMT7 = new Location(224, "salatiga");
            Location zone225_GMT7 = new Location(225, "samarinda");
            Location zone226_GMT7 = new Location(226, "sampang");
            Location zone227_GMT7 = new Location(227, "sampit");
            Location zone228_GMT7 = new Location(228, "sanggau");
            Location zone229_GMT7 = new Location(229, "sawahlunto");
            Location zone230_GMT7 = new Location(230, "sekayu");
            Location zone232_GMT7 = new Location(232, "semarang");
            Location zone234_GMT7 = new Location(234, "serang");
            Location zone236_GMT7 = new Location(236, "sibolga");
            Location zone237_GMT7 = new Location(237, "sidikalang");
            Location zone238_GMT7 = new Location(238, "sidoarjo");
            Location zone239_GMT7 = new Location(239, "sigli");
            Location zone240_GMT7 = new Location(240, "singaparna");
            Location zone242_GMT7 = new Location(242, "singkawang");
            Location zone244_GMT7 = new Location(244, "sintang");
            Location zone245_GMT7 = new Location(245, "situbondo");
            Location zone246_GMT7 = new Location(246, "slawi");
            Location zone247_GMT7 = new Location(247, "sleman");
            Location zone250_GMT7 = new Location(250, "solo");
            Location zone251_GMT7 = new Location(251, "solok");
            Location zone252_GMT7 = new Location(252, "soreang");
            Location zone254_GMT7 = new Location(254, "sragen");
            Location zone255_GMT7 = new Location(255, "stabat");
            Location zone256_GMT7 = new Location(256, "subang");
            Location zone257_GMT7 = new Location(257, "sukabumi");
            Location zone258_GMT7 = new Location(258, "sukoharjo");
            Location zone261_GMT7 = new Location(261, "sumenep");
            Location zone262_GMT7 = new Location(262, "sungailiat");
            Location zone263_GMT7 = new Location(263, "sungaipenuh");
            Location zone265_GMT7 = new Location(265, "surabaya");
            Location zone266_GMT7 = new Location(266, "surakarta");
            Location zone270_GMT7 = new Location(270, "takengon");
            Location zone273_GMT7 = new Location(273, "tangerang");
            Location zone274_GMT7 = new Location(274, "tanjungbalai");
            Location zone275_GMT7 = new Location(275, "tanjungenim");
            Location zone276_GMT7 = new Location(276, "tanjungpandan");
            Location zone277_GMT7 = new Location(277, "tanjungpinang");
            Location zone278_GMT7 = new Location(278, "tanjungredep");
            Location zone279_GMT7 = new Location(279, "tanjungselor");
            Location zone280_GMT7 = new Location(280, "tapaktuan");
            Location zone282_GMT7 = new Location(282, "tarutung");
            Location zone283_GMT7 = new Location(283, "tasikmalaya");
            Location zone284_GMT7 = new Location(284, "tebingtinggi");
            Location zone285_GMT7 = new Location(285, "tegal");
            Location zone286_GMT7 = new Location(286, "temanggung");
            Location zone287_GMT7 = new Location(287, "tembilahan");
            Location zone288_GMT7 = new Location(288, "tenggarong");
            Location zone292_GMT7 = new Location(292, "trenggalek");
            Location zone294_GMT7 = new Location(294, "tuban");
            Location zone295_GMT7 = new Location(295, "tulungagung");
            Location zone296_GMT7 = new Location(296, "ujungberung");
            Location zone297_GMT7 = new Location(297, "ungaran");
            Location zone303_GMT7 = new Location(303, "wates");
            Location zone304_GMT7 = new Location(304, "wonogiri");
            Location zone305_GMT7 = new Location(305, "wonosari");
            Location zone306_GMT7 = new Location(306, "wonosobo");
            Location zone307_GMT7 = new Location(307, "Ciamis");
            Location zone308_GMT7 = new Location(308, "Yogyakarta");
            

            if (!locations7.ContainsKey(zone4_GMT7.Zone))
            {
                locations7.Add(zone4_GMT7.Zone, new List<Location> { zone4_GMT7 });
                locations7.Add(zone1_GMT7.Zone, new List<Location> { zone1_GMT7 });
                locations7.Add(zone7_GMT7.Zone, new List<Location> { zone7_GMT7 });
                locations7.Add(zone9_GMT7.Zone, new List<Location> { zone9_GMT7 });
                locations7.Add(zone10_GMT7.Zone, new List<Location> { zone10_GMT7 });
                locations7.Add(zone11_GMT7.Zone, new List<Location> { zone11_GMT7 });
                locations7.Add(zone12_GMT7.Zone, new List<Location> { zone12_GMT7 });
                locations7.Add(zone13_GMT7.Zone, new List<Location> { zone13_GMT7 });
                locations7.Add(zone14_GMT7.Zone, new List<Location> { zone14_GMT7 });
                locations7.Add(zone15_GMT7.Zone, new List<Location> { zone15_GMT7 });
                locations7.Add(zone16_GMT7.Zone, new List<Location> { zone16_GMT7 });
                locations7.Add(zone18_GMT7.Zone, new List<Location> { zone18_GMT7 });
                locations7.Add(zone19_GMT7.Zone, new List<Location> { zone19_GMT7 });
                locations7.Add(zone21_GMT7.Zone, new List<Location> { zone21_GMT7 });
                locations7.Add(zone23_GMT7.Zone, new List<Location> { zone23_GMT7 });
                locations7.Add(zone24_GMT7.Zone, new List<Location> { zone24_GMT7 });
                locations7.Add(zone25_GMT7.Zone, new List<Location> { zone25_GMT7 });
                locations7.Add(zone29_GMT7.Zone, new List<Location> { zone29_GMT7 });
                locations7.Add(zone30_GMT7.Zone, new List<Location> { zone30_GMT7 });
                locations7.Add(zone31_GMT7.Zone, new List<Location> { zone31_GMT7 });
                locations7.Add(zone32_GMT7.Zone, new List<Location> { zone32_GMT7 });
                locations7.Add(zone33_GMT7.Zone, new List<Location> { zone33_GMT7 });
                locations7.Add(zone35_GMT7.Zone, new List<Location> { zone35_GMT7 });
                locations7.Add(zone36_GMT7.Zone, new List<Location> { zone36_GMT7 });
                locations7.Add(zone37_GMT7.Zone, new List<Location> { zone37_GMT7 });
                locations7.Add(zone41_GMT7.Zone, new List<Location> { zone41_GMT7 });
                locations7.Add(zone42_GMT7.Zone, new List<Location> { zone42_GMT7 });
                locations7.Add(zone44_GMT7.Zone, new List<Location> { zone44_GMT7 });
                locations7.Add(zone45_GMT7.Zone, new List<Location> { zone45_GMT7 });
                locations7.Add(zone46_GMT7.Zone, new List<Location> { zone46_GMT7 });
                locations7.Add(zone47_GMT7.Zone, new List<Location> { zone47_GMT7 });
                locations7.Add(zone48_GMT7.Zone, new List<Location> { zone48_GMT7 });
                locations7.Add(zone50_GMT7.Zone, new List<Location> { zone50_GMT7 });
                locations7.Add(zone51_GMT7.Zone, new List<Location> { zone51_GMT7 });
                locations7.Add(zone52_GMT7.Zone, new List<Location> { zone52_GMT7 });
                locations7.Add(zone54_GMT7.Zone, new List<Location> { zone54_GMT7 });
                locations7.Add(zone55_GMT7.Zone, new List<Location> { zone55_GMT7 });
                locations7.Add(zone56_GMT7.Zone, new List<Location> { zone56_GMT7 });
                locations7.Add(zone57_GMT7.Zone, new List<Location> { zone57_GMT7 });
                locations7.Add(zone58_GMT7.Zone, new List<Location> { zone58_GMT7 });
                locations7.Add(zone59_GMT7.Zone, new List<Location> { zone59_GMT7 });
                locations7.Add(zone60_GMT7.Zone, new List<Location> { zone60_GMT7 });
                locations7.Add(zone61_GMT7.Zone, new List<Location> { zone61_GMT7 });
                locations7.Add(zone62_GMT7.Zone, new List<Location> { zone62_GMT7 });
                locations7.Add(zone63_GMT7.Zone, new List<Location> { zone63_GMT7 });
                locations7.Add(zone65_GMT7.Zone, new List<Location> { zone65_GMT7 });
                locations7.Add(zone69_GMT7.Zone, new List<Location> { zone69_GMT7 });
                locations7.Add(zone71_GMT7.Zone, new List<Location> { zone71_GMT7 });
                locations7.Add(zone74_GMT7.Zone, new List<Location> { zone74_GMT7 });
                locations7.Add(zone75_GMT7.Zone, new List<Location> { zone75_GMT7 });
                locations7.Add(zone76_GMT7.Zone, new List<Location> { zone76_GMT7 });
                locations7.Add(zone78_GMT7.Zone, new List<Location> { zone78_GMT7 });
                locations7.Add(zone79_GMT7.Zone, new List<Location> { zone79_GMT7 });
                locations7.Add(zone80_GMT7.Zone, new List<Location> { zone80_GMT7 });
                locations7.Add(zone81_GMT7.Zone, new List<Location> { zone81_GMT7 });
                locations7.Add(zone82_GMT7.Zone, new List<Location> { zone82_GMT7 });
                locations7.Add(zone84_GMT7.Zone, new List<Location> { zone84_GMT7 });
                locations7.Add(zone86_GMT7.Zone, new List<Location> { zone86_GMT7 });
                locations7.Add(zone87_GMT7.Zone, new List<Location> { zone87_GMT7 });
                locations7.Add(zone88_GMT7.Zone, new List<Location> { zone88_GMT7 });
                locations7.Add(zone90_GMT7.Zone, new List<Location> { zone90_GMT7 });
                locations7.Add(zone92_GMT7.Zone, new List<Location> { zone92_GMT7 });
                locations7.Add(zone93_GMT7.Zone, new List<Location> { zone93_GMT7 });
                locations7.Add(zone95_GMT7.Zone, new List<Location> { zone95_GMT7 });
                locations7.Add(zone96_GMT7.Zone, new List<Location> { zone96_GMT7 });
                locations7.Add(zone97_GMT7.Zone, new List<Location> { zone97_GMT7 });
                locations7.Add(zone100_GMT7.Zone, new List<Location> { zone100_GMT7 });
                locations7.Add(zone102_GMT7.Zone, new List<Location> { zone102_GMT7 });
                locations7.Add(zone103_GMT7.Zone, new List<Location> { zone103_GMT7 });
                locations7.Add(zone104_GMT7.Zone, new List<Location> { zone104_GMT7 });
                locations7.Add(zone105_GMT7.Zone, new List<Location> { zone105_GMT7 });
                locations7.Add(zone108_GMT7.Zone, new List<Location> { zone108_GMT7 });
                locations7.Add(zone109_GMT7.Zone, new List<Location> { zone109_GMT7 });
                locations7.Add(zone111_GMT7.Zone, new List<Location> { zone111_GMT7 });
                locations7.Add(zone112_GMT7.Zone, new List<Location> { zone112_GMT7 });
                locations7.Add(zone113_GMT7.Zone, new List<Location> { zone113_GMT7 });
                locations7.Add(zone114_GMT7.Zone, new List<Location> { zone114_GMT7 });
                locations7.Add(zone115_GMT7.Zone, new List<Location> { zone115_GMT7 });
                locations7.Add(zone116_GMT7.Zone, new List<Location> { zone116_GMT7 });
                locations7.Add(zone118_GMT7.Zone, new List<Location> { zone118_GMT7 });
                locations7.Add(zone119_GMT7.Zone, new List<Location> { zone119_GMT7 });
                locations7.Add(zone120_GMT7.Zone, new List<Location> { zone120_GMT7 });
                locations7.Add(zone121_GMT7.Zone, new List<Location> { zone121_GMT7 });
                locations7.Add(zone122_GMT7.Zone, new List<Location> { zone122_GMT7 });
                locations7.Add(zone123_GMT7.Zone, new List<Location> { zone123_GMT7 });
                locations7.Add(zone125_GMT7.Zone, new List<Location> { zone125_GMT7 });
                locations7.Add(zone126_GMT7.Zone, new List<Location> { zone126_GMT7 });
                locations7.Add(zone128_GMT7.Zone, new List<Location> { zone128_GMT7 });
                locations7.Add(zone129_GMT7.Zone, new List<Location> { zone129_GMT7 });
                locations7.Add(zone130_GMT7.Zone, new List<Location> { zone130_GMT7 });
                locations7.Add(zone131_GMT7.Zone, new List<Location> { zone131_GMT7 });
                locations7.Add(zone132_GMT7.Zone, new List<Location> { zone132_GMT7 });
                locations7.Add(zone134_GMT7.Zone, new List<Location> { zone134_GMT7 });
                locations7.Add(zone135_GMT7.Zone, new List<Location> { zone135_GMT7 });
                locations7.Add(zone136_GMT7.Zone, new List<Location> { zone136_GMT7 });
                locations7.Add(zone137_GMT7.Zone, new List<Location> { zone137_GMT7 });
                locations7.Add(zone141_GMT7.Zone, new List<Location> { zone141_GMT7 });
                locations7.Add(zone143_GMT7.Zone, new List<Location> { zone143_GMT7 });
                locations7.Add(zone151_GMT7.Zone, new List<Location> { zone151_GMT7 });
                locations7.Add(zone152_GMT7.Zone, new List<Location> { zone152_GMT7 });
                locations7.Add(zone154_GMT7.Zone, new List<Location> { zone154_GMT7 });
                locations7.Add(zone156_GMT7.Zone, new List<Location> { zone156_GMT7 });
                locations7.Add(zone157_GMT7.Zone, new List<Location> { zone157_GMT7 });
                locations7.Add(zone158_GMT7.Zone, new List<Location> { zone158_GMT7 });
                locations7.Add(zone159_GMT7.Zone, new List<Location> { zone159_GMT7 });
                locations7.Add(zone161_GMT7.Zone, new List<Location> { zone161_GMT7 });
                locations7.Add(zone160_GMT7.Zone, new List<Location> { zone160_GMT7 });
                locations7.Add(zone162_GMT7.Zone, new List<Location> { zone162_GMT7 });
                locations7.Add(zone163_GMT7.Zone, new List<Location> { zone163_GMT7 });
                locations7.Add(zone164_GMT7.Zone, new List<Location> { zone164_GMT7 });
                locations7.Add(zone167_GMT7.Zone, new List<Location> { zone167_GMT7 });
                locations7.Add(zone168_GMT7.Zone, new List<Location> { zone168_GMT7 });
                locations7.Add(zone170_GMT7.Zone, new List<Location> { zone170_GMT7 });
                locations7.Add(zone171_GMT7.Zone, new List<Location> { zone171_GMT7 });
                locations7.Add(zone172_GMT7.Zone, new List<Location> { zone172_GMT7 });
                locations7.Add(zone173_GMT7.Zone, new List<Location> { zone173_GMT7 });
                locations7.Add(zone174_GMT7.Zone, new List<Location> { zone174_GMT7 });
                locations7.Add(zone175_GMT7.Zone, new List<Location> { zone175_GMT7 });
                locations7.Add(zone180_GMT7.Zone, new List<Location> { zone180_GMT7 });
                locations7.Add(zone181_GMT7.Zone, new List<Location> { zone181_GMT7 });
                locations7.Add(zone184_GMT7.Zone, new List<Location> { zone184_GMT7 });
                locations7.Add(zone185_GMT7.Zone, new List<Location> { zone185_GMT7 });
                locations7.Add(zone186_GMT7.Zone, new List<Location> { zone186_GMT7 });
                locations7.Add(zone187_GMT7.Zone, new List<Location> { zone187_GMT7 });
                locations7.Add(zone189_GMT7.Zone, new List<Location> { zone189_GMT7 });
                locations7.Add(zone190_GMT7.Zone, new List<Location> { zone190_GMT7 });
                locations7.Add(zone191_GMT7.Zone, new List<Location> { zone191_GMT7 });
                locations7.Add(zone192_GMT7.Zone, new List<Location> { zone192_GMT7 });
                locations7.Add(zone193_GMT7.Zone, new List<Location> { zone193_GMT7 });
                locations7.Add(zone194_GMT7.Zone, new List<Location> { zone194_GMT7 });
                locations7.Add(zone195_GMT7.Zone, new List<Location> { zone195_GMT7 });
                locations7.Add(zone196_GMT7.Zone, new List<Location> { zone196_GMT7 });
                locations7.Add(zone197_GMT7.Zone, new List<Location> { zone197_GMT7 });
                locations7.Add(zone201_GMT7.Zone, new List<Location> { zone201_GMT7 });
                locations7.Add(zone202_GMT7.Zone, new List<Location> { zone202_GMT7 });
                locations7.Add(zone203_GMT7.Zone, new List<Location> { zone203_GMT7 });
                locations7.Add(zone206_GMT7.Zone, new List<Location> { zone206_GMT7 });
                locations7.Add(zone205_GMT7.Zone, new List<Location> { zone205_GMT7 });
                locations7.Add(zone207_GMT7.Zone, new List<Location> { zone207_GMT7 });
                locations7.Add(zone208_GMT7.Zone, new List<Location> { zone208_GMT7 });
                locations7.Add(zone209_GMT7.Zone, new List<Location> { zone209_GMT7 });
                locations7.Add(zone210_GMT7.Zone, new List<Location> { zone210_GMT7 });
                locations7.Add(zone211_GMT7.Zone, new List<Location> { zone211_GMT7 });
                locations7.Add(zone212_GMT7.Zone, new List<Location> { zone212_GMT7 });
                locations7.Add(zone213_GMT7.Zone, new List<Location> { zone213_GMT7 });
                locations7.Add(zone214_GMT7.Zone, new List<Location> { zone214_GMT7 });
                locations7.Add(zone216_GMT7.Zone, new List<Location> { zone216_GMT7 });
                locations7.Add(zone218_GMT7.Zone, new List<Location> { zone218_GMT7 });
                locations7.Add(zone220_GMT7.Zone, new List<Location> { zone220_GMT7 });
                locations7.Add(zone221_GMT7.Zone, new List<Location> { zone221_GMT7 });
                locations7.Add(zone223_GMT7.Zone, new List<Location> { zone223_GMT7 });
                locations7.Add(zone224_GMT7.Zone, new List<Location> { zone224_GMT7 });
                locations7.Add(zone225_GMT7.Zone, new List<Location> { zone225_GMT7 });
                locations7.Add(zone226_GMT7.Zone, new List<Location> { zone226_GMT7 });
                locations7.Add(zone227_GMT7.Zone, new List<Location> { zone227_GMT7 });
                locations7.Add(zone228_GMT7.Zone, new List<Location> { zone228_GMT7 });
                locations7.Add(zone229_GMT7.Zone, new List<Location> { zone229_GMT7 });
                locations7.Add(zone230_GMT7.Zone, new List<Location> { zone230_GMT7 });
                locations7.Add(zone232_GMT7.Zone, new List<Location> { zone232_GMT7 });
                locations7.Add(zone234_GMT7.Zone, new List<Location> { zone234_GMT7 });
                locations7.Add(zone236_GMT7.Zone, new List<Location> { zone236_GMT7 });
                locations7.Add(zone237_GMT7.Zone, new List<Location> { zone237_GMT7 });
                locations7.Add(zone238_GMT7.Zone, new List<Location> { zone238_GMT7 });
                locations7.Add(zone239_GMT7.Zone, new List<Location> { zone239_GMT7 });
                locations7.Add(zone240_GMT7.Zone, new List<Location> { zone240_GMT7 });
                locations7.Add(zone242_GMT7.Zone, new List<Location> { zone242_GMT7 });
                locations7.Add(zone244_GMT7.Zone, new List<Location> { zone244_GMT7 });
                locations7.Add(zone246_GMT7.Zone, new List<Location> { zone246_GMT7 });
                locations7.Add(zone247_GMT7.Zone, new List<Location> { zone247_GMT7 });
                locations7.Add(zone250_GMT7.Zone, new List<Location> { zone250_GMT7 });
                locations7.Add(zone251_GMT7.Zone, new List<Location> { zone251_GMT7 });
                locations7.Add(zone252_GMT7.Zone, new List<Location> { zone252_GMT7 });
                locations7.Add(zone254_GMT7.Zone, new List<Location> { zone254_GMT7 });
                locations7.Add(zone255_GMT7.Zone, new List<Location> { zone255_GMT7 });
                locations7.Add(zone256_GMT7.Zone, new List<Location> { zone256_GMT7 });
                locations7.Add(zone257_GMT7.Zone, new List<Location> { zone257_GMT7 });
                locations7.Add(zone258_GMT7.Zone, new List<Location> { zone258_GMT7 });
                locations7.Add(zone261_GMT7.Zone, new List<Location> { zone261_GMT7 });
                locations7.Add(zone245_GMT7.Zone, new List<Location> { zone245_GMT7 });
                locations7.Add(zone262_GMT7.Zone, new List<Location> { zone262_GMT7 });
                locations7.Add(zone263_GMT7.Zone, new List<Location> { zone263_GMT7 });
                locations7.Add(zone265_GMT7.Zone, new List<Location> { zone265_GMT7 });
                locations7.Add(zone266_GMT7.Zone, new List<Location> { zone266_GMT7 });
                locations7.Add(zone270_GMT7.Zone, new List<Location> { zone270_GMT7 });
                locations7.Add(zone273_GMT7.Zone, new List<Location> { zone273_GMT7 });
                locations7.Add(zone274_GMT7.Zone, new List<Location> { zone274_GMT7 });
                locations7.Add(zone275_GMT7.Zone, new List<Location> { zone275_GMT7 });
                locations7.Add(zone276_GMT7.Zone, new List<Location> { zone276_GMT7 });
                locations7.Add(zone277_GMT7.Zone, new List<Location> { zone277_GMT7 });
                locations7.Add(zone278_GMT7.Zone, new List<Location> { zone278_GMT7 });
                locations7.Add(zone279_GMT7.Zone, new List<Location> { zone279_GMT7 });
                locations7.Add(zone280_GMT7.Zone, new List<Location> { zone280_GMT7 });
                locations7.Add(zone282_GMT7.Zone, new List<Location> { zone282_GMT7 });
                locations7.Add(zone284_GMT7.Zone, new List<Location> { zone284_GMT7 });
                locations7.Add(zone285_GMT7.Zone, new List<Location> { zone285_GMT7 });
                locations7.Add(zone286_GMT7.Zone, new List<Location> { zone286_GMT7 });
                locations7.Add(zone287_GMT7.Zone, new List<Location> { zone287_GMT7 });
                locations7.Add(zone283_GMT7.Zone, new List<Location> { zone283_GMT7 });
                locations7.Add(zone288_GMT7.Zone, new List<Location> { zone288_GMT7 });
                locations7.Add(zone292_GMT7.Zone, new List<Location> { zone292_GMT7 });
                locations7.Add(zone294_GMT7.Zone, new List<Location> { zone294_GMT7 });
                locations7.Add(zone295_GMT7.Zone, new List<Location> { zone295_GMT7 });
                locations7.Add(zone296_GMT7.Zone, new List<Location> { zone296_GMT7 });
                locations7.Add(zone297_GMT7.Zone, new List<Location> { zone297_GMT7 });
                locations7.Add(zone303_GMT7.Zone, new List<Location> { zone303_GMT7 });
                locations7.Add(zone304_GMT7.Zone, new List<Location> { zone304_GMT7 });
                locations7.Add(zone305_GMT7.Zone, new List<Location> { zone305_GMT7 });
                locations7.Add(zone306_GMT7.Zone, new List<Location> { zone306_GMT7 });
                locations7.Add(zone307_GMT7.Zone, new List<Location> { zone307_GMT7 });
                locations7.Add(zone308_GMT7.Zone, new List<Location> { zone308_GMT7 });
            }

            Location zone7_GMT9 = new Location(98, "Babo");
            Location zone5_GMT9 = new Location(5, "Ambon");
            Location zone6_GMT9 = new Location(6, "Atambua");
            Location zone34_GMT9 = new Location(34, "Baubau");
            Location zone38_GMT9 = new Location(38, "Benteng");
            Location zone39_GMT9 = new Location(39, "Biak");
            Location zone43_GMT9 = new Location(43, "Bitung");
            Location zone66_GMT9 = new Location(66, "dili");
            Location zone73_GMT9 = new Location(73, "fakfak");
            Location zone83_GMT9 = new Location(83, "jayapura");
            Location zone144_GMT9 = new Location(144, "manokwari");
            Location zone148_GMT9 = new Location(148, "masohi");
            Location zone155_GMT9 = new Location(155, "merauke");
            Location zone165_GMT9 = new Location(165, "nabire");
            Location zone235_GMT9 = new Location(235, "serui");
            Location zone253_GMT9 = new Location(253, "sorong");
            Location zone289_GMT9 = new Location(289, "ternate");
            Location zone293_GMT9 = new Location(293, "tual");
            Location zone299_GMT9 = new Location(299, "waingapu");

            if (!locations9.ContainsKey(zone5_GMT9.Zone))
            {
                locations9.Add(zone5_GMT9.Zone, new List<Location> { zone5_GMT9 });
                locations9.Add(zone6_GMT9.Zone, new List<Location> { zone6_GMT9 });
                locations9.Add(zone7_GMT9.Zone, new List<Location> { zone7_GMT9 });
                locations9.Add(zone34_GMT9.Zone, new List<Location> { zone34_GMT9 });
                locations9.Add(zone38_GMT9.Zone, new List<Location> { zone38_GMT9 });
                locations9.Add(zone39_GMT9.Zone, new List<Location> { zone39_GMT9 });
                locations9.Add(zone43_GMT9.Zone, new List<Location> { zone43_GMT9 });
                locations9.Add(zone66_GMT9.Zone, new List<Location> { zone66_GMT9 });
                locations9.Add(zone73_GMT9.Zone, new List<Location> { zone73_GMT9 });
                locations9.Add(zone83_GMT9.Zone, new List<Location> { zone83_GMT9 });
                locations9.Add(zone144_GMT9.Zone, new List<Location> { zone144_GMT9 });
                locations9.Add(zone148_GMT9.Zone, new List<Location> { zone148_GMT9 });
                locations9.Add(zone155_GMT9.Zone, new List<Location> { zone155_GMT9 });
                locations9.Add(zone165_GMT9.Zone, new List<Location> { zone165_GMT9 });
                locations9.Add(zone235_GMT9.Zone, new List<Location> { zone235_GMT9 });
                locations9.Add(zone253_GMT9.Zone, new List<Location> { zone253_GMT9 });
                locations9.Add(zone289_GMT9.Zone, new List<Location> { zone289_GMT9 });
                locations9.Add(zone293_GMT9.Zone, new List<Location> { zone293_GMT9 });
                locations9.Add(zone299_GMT9.Zone, new List<Location> { zone299_GMT9 });
            }          
        }
        private void AddingDateTo_DicTionary()
        {
            foreach (var zone in locations8.Values.ToList())
            {
                zone[0].date = date;
            }
            getDatabase_GMT8Zone();
            foreach (var zone in locations7.Values.ToList())
            {
                zone[0].date = dateGMT7;
            }
            getDatabase_GMT7Zone();
            foreach (var zone in locations9.Values.ToList())
            {
                zone[0].date = dateGMT9;
            }
            getDatabase_GMT9Zone();
        }
        private void getDatabase_GMT8Zone()
        {
            foreach (var Area in locations8.Values.ToList())
            {
                if (Area[0].date != null)
                  getzonedata(Area[0].zone, Area[0].location.ToString(), Area[0].date.ToString());                
            }
        }
        private void getDatabase_GMT7Zone()
        {
            foreach (var Area in locations7.Values.ToList())
            {
                if (Area[0].date != null)
                getzonedata(Area[0].zone, Area[0].location.ToString(), Area[0].date.ToString());
            }
        }
        private void getDatabase_GMT9Zone()
        {
            foreach (var Area in locations9.Values.ToList())
            {
                if (Area[0].date != null)
                   getzonedata(Area[0].zone, Area[0].location.ToString(), Area[0].date.ToString());
            }
        }
        private void getzonedata(int zonenum, string mzone, string date)
        {
            connetionString = "SERVER=" + server + ";" + "DATABASE=" + database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            using (cnn = new MySqlConnection(connetionString))
            {
                cnn.Open();
                string sql = "Select `Tarikh Miladi`, Subuh, Zohor, Asar, Maghrib, Isyak  from " + mzone + " where `Tarikh Miladi`  = '" + date + "'";

                using (command = new MySqlCommand(sql, cnn))
                {
                    dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        if (!azanTimeMethods9.ContainsKey(zonenum) == locations9.ContainsKey(zonenum))
                        {
                            azanTimeMethods9.Add
                            (zonenum, new List<AzanTimeMethod>{ new AzanTimeMethod{
                            zoneNum = zonenum,
                            NameZone = mzone ,
                            TarikhMiladi = dataReader.GetValue(0).ToString(),
                            Subuh = dataReader.GetValue(1).ToString(),
                            Zohor = dataReader.GetValue(2).ToString(),
                            Asar = dataReader.GetValue(3).ToString(),
                            Maghrib = dataReader.GetValue(4).ToString(),
                            Isyak = dataReader.GetValue(5).ToString()}
                            });
                        }
                        if (!azanTimeMethods7.ContainsKey(zonenum) == locations7.ContainsKey(zonenum))
                        {
                            azanTimeMethods7.Add
                            (zonenum, new List<AzanTimeMethod>{ new AzanTimeMethod{
                            zoneNum = zonenum,
                            NameZone = mzone ,
                            TarikhMiladi = dataReader.GetValue(0).ToString(),
                            Subuh = dataReader.GetValue(1).ToString(),
                            Zohor = dataReader.GetValue(2).ToString(),
                            Asar = dataReader.GetValue(3).ToString(),
                            Maghrib = dataReader.GetValue(4).ToString(),
                            Isyak = dataReader.GetValue(5).ToString()}
                            });
                        }
                        if (!azanTimeMethods8.ContainsKey(zonenum) == locations8.ContainsKey(zonenum))
                        {
                            azanTimeMethods8.Add
                            (zonenum, new List<AzanTimeMethod>{ new AzanTimeMethod{
                            zoneNum = zonenum,
                            NameZone = mzone ,
                            TarikhMiladi = dataReader.GetValue(0).ToString(),
                            Subuh = dataReader.GetValue(1).ToString(),
                            Zohor = dataReader.GetValue(2).ToString(),
                            Asar = dataReader.GetValue(3).ToString(),
                            Maghrib = dataReader.GetValue(4).ToString(),
                            Isyak = dataReader.GetValue(5).ToString()}
                            });
                        }
                    }

                }
            }
           
        }
        private async Task setReminder15minDictionary()
        {
            foreach (var dateTime in azanTimeMethods8.Values)
            {
                if (!timeMinus15min8.ContainsKey(dateTime[0].zoneNum))

                    timeMinus15min8.Add(dateTime[0].zoneNum, new List<AzanTimeMethod15mins>()
                    { new AzanTimeMethod15mins
                    {
                      TarikhMiladi = dateTime[0].TarikhMiladi,
                      zoneNum =  dateTime[0].zoneNum,
                      NameZone = dateTime[0].NameZone,
                      Subuh = Convert.ToDateTime(dateTime[0].Subuh).AddMinutes(-15).ToShortTimeString(),
                      Zohor = Convert.ToDateTime(dateTime[0].Zohor).AddMinutes(-15).ToShortTimeString(),
                      Asar = Convert.ToDateTime(dateTime[0].Asar).AddMinutes(-15).ToShortTimeString(),
                      Maghrib = Convert.ToDateTime(dateTime[0].Maghrib).AddMinutes(-15).ToShortTimeString(),
                      Isyak =Convert.ToDateTime(dateTime[0].Isyak).AddMinutes(-15).ToShortTimeString()
                    }});
            }
            foreach (var dateTime in azanTimeMethods7.Values)
            {
                if (!timeMinus15min7.ContainsKey(dateTime[0].zoneNum))

                    timeMinus15min7.Add(dateTime[0].zoneNum, new List<AzanTimeMethod15mins>()
                    { new AzanTimeMethod15mins
                    {
                      TarikhMiladi = dateTime[0].TarikhMiladi,
                      zoneNum =  dateTime[0].zoneNum,
                      NameZone = dateTime[0].NameZone,
                      Subuh = Convert.ToDateTime(dateTime[0].Subuh).AddMinutes(-15).ToShortTimeString(),
                      Zohor = Convert.ToDateTime(dateTime[0].Zohor).AddMinutes(-15).ToShortTimeString(),
                      Asar = Convert.ToDateTime(dateTime[0].Asar).AddMinutes(-15).ToShortTimeString(),
                      Maghrib = Convert.ToDateTime(dateTime[0].Maghrib).AddMinutes(-15).ToShortTimeString(),
                      Isyak =Convert.ToDateTime(dateTime[0].Isyak).AddMinutes(-15).ToShortTimeString()
                    }});
            }
            foreach (var dateTime in azanTimeMethods9.Values)
            {
                if (!timeMinus15min9.ContainsKey(dateTime[0].zoneNum))

                    timeMinus15min9.Add(dateTime[0].zoneNum, new List<AzanTimeMethod15mins>()
                    { new AzanTimeMethod15mins
                    {
                      TarikhMiladi = dateTime[0].TarikhMiladi,
                      zoneNum =  dateTime[0].zoneNum,
                      NameZone = dateTime[0].NameZone,
                      Subuh = Convert.ToDateTime(dateTime[0].Subuh).AddMinutes(-15).ToShortTimeString(),
                      Zohor = Convert.ToDateTime(dateTime[0].Zohor).AddMinutes(-15).ToShortTimeString(),
                      Asar = Convert.ToDateTime(dateTime[0].Asar).AddMinutes(-15).ToShortTimeString(),
                      Maghrib = Convert.ToDateTime(dateTime[0].Maghrib).AddMinutes(-15).ToShortTimeString(),
                      Isyak =Convert.ToDateTime(dateTime[0].Isyak).AddMinutes(-15).ToShortTimeString()
                    }});
            }
            await Task.Delay(1000);
        }
        private async Task  setReminder10minDictionary()
        {
            foreach (var dateTime in azanTimeMethods8.Values)
            {
                if (!timeMinus10min8.ContainsKey(dateTime[0].zoneNum))

                    timeMinus10min8.Add(dateTime[0].zoneNum, new List<AzanTimeMethod10mins>()
                    { new AzanTimeMethod10mins
                    {
                      TarikhMiladi = dateTime[0].TarikhMiladi,
                      zoneNum =  dateTime[0].zoneNum,
                      NameZone = dateTime[0].NameZone,
                      Subuh = Convert.ToDateTime(dateTime[0].Subuh).AddMinutes(-10).ToShortTimeString(),
                      Zohor = Convert.ToDateTime(dateTime[0].Zohor).AddMinutes(-10).ToShortTimeString(),
                      Asar = Convert.ToDateTime(dateTime[0].Asar).AddMinutes(-10).ToShortTimeString(),
                      Maghrib = Convert.ToDateTime(dateTime[0].Maghrib).AddMinutes(-10).ToShortTimeString(),
                      Isyak =Convert.ToDateTime(dateTime[0].Isyak).AddMinutes(-10).ToShortTimeString()
                    }});
            }
            foreach (var dateTime in azanTimeMethods7.Values)
            {
                if (!timeMinus10min7.ContainsKey(dateTime[0].zoneNum))

                    timeMinus10min7.Add(dateTime[0].zoneNum, new List<AzanTimeMethod10mins>()
                    { new AzanTimeMethod10mins
                    {
                      TarikhMiladi = dateTime[0].TarikhMiladi,
                      zoneNum =  dateTime[0].zoneNum,
                      NameZone = dateTime[0].NameZone,
                      Subuh = Convert.ToDateTime(dateTime[0].Subuh).AddMinutes(-10).ToShortTimeString(),
                      Zohor = Convert.ToDateTime(dateTime[0].Zohor).AddMinutes(-10).ToShortTimeString(),
                      Asar = Convert.ToDateTime(dateTime[0].Asar).AddMinutes(-10).ToShortTimeString(),
                      Maghrib = Convert.ToDateTime(dateTime[0].Maghrib).AddMinutes(-10).ToShortTimeString(),
                      Isyak =Convert.ToDateTime(dateTime[0].Isyak).AddMinutes(-10).ToShortTimeString()
                    }});
            }
            foreach (var dateTime in azanTimeMethods9.Values)
            {
                if (!timeMinus10min9.ContainsKey(dateTime[0].zoneNum))

                    timeMinus10min9.Add(dateTime[0].zoneNum, new List<AzanTimeMethod10mins>()
                    { new AzanTimeMethod10mins
                    {
                      TarikhMiladi = dateTime[0].TarikhMiladi,
                      zoneNum =  dateTime[0].zoneNum,
                      NameZone = dateTime[0].NameZone,
                      Subuh = Convert.ToDateTime(dateTime[0].Subuh).AddMinutes(-10).ToShortTimeString(),
                      Zohor = Convert.ToDateTime(dateTime[0].Zohor).AddMinutes(-10).ToShortTimeString(),
                      Asar = Convert.ToDateTime(dateTime[0].Asar).AddMinutes(-10).ToShortTimeString(),
                      Maghrib = Convert.ToDateTime(dateTime[0].Maghrib).AddMinutes(-10).ToShortTimeString(),
                      Isyak =Convert.ToDateTime(dateTime[0].Isyak).AddMinutes(-10).ToShortTimeString()
                    }});
            }
            await Task.Delay(1000);
        }
        private async Task setReminder5minDictionary()
        {
            foreach (var dateTime in azanTimeMethods8.Values)
            {
                if (!timeMinus5min8.ContainsKey(dateTime[0].zoneNum))

                    timeMinus5min8.Add(dateTime[0].zoneNum, new List<AzanTimeMethod5min>()
                    { new AzanTimeMethod5min
                    {
                      TarikhMiladi = dateTime[0].TarikhMiladi,
                      zoneNum =  dateTime[0].zoneNum,
                      NameZone = dateTime[0].NameZone,
                      Subuh = Convert.ToDateTime(dateTime[0].Subuh).AddMinutes(-5).ToShortTimeString(),
                      Zohor = Convert.ToDateTime(dateTime[0].Zohor).AddMinutes(-5).ToShortTimeString(),
                      Asar = Convert.ToDateTime(dateTime[0].Asar).AddMinutes(-5).ToShortTimeString(),
                      Maghrib = Convert.ToDateTime(dateTime[0].Maghrib).AddMinutes(-5).ToShortTimeString(),
                      Isyak =Convert.ToDateTime(dateTime[0].Isyak).AddMinutes(-5).ToShortTimeString()
                    }});
            }
            foreach (var dateTime in azanTimeMethods7.Values)
            {
                if (!timeMinus5min7.ContainsKey(dateTime[0].zoneNum))

                    timeMinus5min7.Add(dateTime[0].zoneNum, new List<AzanTimeMethod5min>()
                    { new AzanTimeMethod5min
                    {
                      TarikhMiladi = dateTime[0].TarikhMiladi,
                      zoneNum =  dateTime[0].zoneNum,
                      NameZone = dateTime[0].NameZone,
                      Subuh = Convert.ToDateTime(dateTime[0].Subuh).AddMinutes(-5).ToShortTimeString(),
                      Zohor = Convert.ToDateTime(dateTime[0].Zohor).AddMinutes(-5).ToShortTimeString(),
                      Asar = Convert.ToDateTime(dateTime[0].Asar).AddMinutes(-5).ToShortTimeString(),
                      Maghrib = Convert.ToDateTime(dateTime[0].Maghrib).AddMinutes(-5).ToShortTimeString(),
                      Isyak =Convert.ToDateTime(dateTime[0].Isyak).AddMinutes(-5).ToShortTimeString()
                    }});
            }
            foreach (var dateTime in azanTimeMethods9.Values)
            {
                if (!timeMinus5min9.ContainsKey(dateTime[0].zoneNum))

                    timeMinus5min9.Add(dateTime[0].zoneNum, new List<AzanTimeMethod5min>()
                    { new AzanTimeMethod5min
                    {
                      TarikhMiladi = dateTime[0].TarikhMiladi,
                      zoneNum =  dateTime[0].zoneNum,
                      NameZone = dateTime[0].NameZone,
                      Subuh = Convert.ToDateTime(dateTime[0].Subuh).AddMinutes(-5).ToShortTimeString(),
                      Zohor = Convert.ToDateTime(dateTime[0].Zohor).AddMinutes(-5).ToShortTimeString(),
                      Asar = Convert.ToDateTime(dateTime[0].Asar).AddMinutes(-5).ToShortTimeString(),
                      Maghrib = Convert.ToDateTime(dateTime[0].Maghrib).AddMinutes(-5).ToShortTimeString(),
                      Isyak =Convert.ToDateTime(dateTime[0].Isyak).AddMinutes(-5).ToShortTimeString()
                    }});
            }
        }
        private async Task Execute_MQtt_15minReminderZone8()
        {
            foreach (var zone in timeMinus15min8.Values.ToList())
            {
                if (zone[0].Subuh == timeGMT8)
                {
                    await Message15minSubuh(zone[0].zoneNum.ToString(), zone[0].Subuh, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Subuh = "Publishsed | " + zone[0].Subuh;         
                    listBox8.Invoke((Action)(() => listBox8.Items.Add(message15minSubuh)));
                    await ConnectMQTT(message15minSubuh);          
                }
                if (zone[0].Zohor == timeGMT8)
                {
                    await Message15minZohor(zone[0].zoneNum.ToString(), zone[0].Zohor, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Zohor = "Publishsed | " + zone[0].Zohor;                 
                    listBox8.Invoke((Action)(() => listBox8.Items.Add(message15minZohor)));
                    await ConnectMQTT(message15minZohor);             
                }
                if (zone[0].Asar == timeGMT8)
                {
                    await Message15minAsar(zone[0].zoneNum.ToString(), zone[0].Asar, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Asar = "Publishsed | " + zone[0].Asar;                
                    listBox8.Invoke((Action)(() => listBox8.Items.Add(message15minAsar)));
                    await ConnectMQTT(message15minAsar);           
                }
                if (zone[0].Maghrib == timeGMT8)
                {
                    await Message15minMaghrib(zone[0].zoneNum.ToString(), zone[0].Maghrib, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Maghrib = "Publishsed | " + zone[0].Maghrib;                  
                    listBox8.Invoke((Action)(() => listBox8.Items.Add(message15minMaghrib)));
                    await ConnectMQTT(message15minMaghrib);               
                }
                if (zone[0].Isyak == timeGMT8)
                {
                    await Message15minIsyak(zone[0].zoneNum.ToString(), zone[0].Isyak, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Isyak = "Publishsed | " + zone[0].Isyak;                 
                    listBox8.Invoke((Action)(() => listBox8.Items.Add(message15minIsyak)));
                    await ConnectMQTT(message15minIsyak);                
                }
            }
        }
        private async Task Execute_MQtt_10minReminderZone8()
        {
            foreach (var zone in timeMinus10min8.Values.ToList())
            {
                if (zone[0].Subuh == timeGMT8)
                {
                    await Message10minSubuh(zone[0].zoneNum.ToString(), zone[0].Subuh, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Subuh = "Publishsed | " + zone[0].Subuh;        
                    listBox8.Invoke((Action)(() => listBox8.Items.Add(message10minSubuh)));
                    await ConnectMQTT(message10minSubuh);            
                }
                if (zone[0].Zohor == timeGMT8)
                {
                    await Message10minZohor(zone[0].zoneNum.ToString(), zone[0].Zohor, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Zohor = "Publishsed | " + zone[0].Zohor;                
                    listBox8.Invoke((Action)(() => listBox8.Items.Add(message10minZohor)));
                    await ConnectMQTT(message10minZohor);           
                }
                if (zone[0].Asar == timeGMT8)
                {
                    await Message10minAsar(zone[0].zoneNum.ToString(), zone[0].Asar, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Asar = "Publishsed | " + zone[0].Asar;             
                    listBox8.Invoke((Action)(() => listBox8.Items.Add(message10minAsar)));
                    await ConnectMQTT(message10minAsar);                   
                }
                if (zone[0].Maghrib == timeGMT8)
                {
                    await Message10minMaghrib(zone[0].zoneNum.ToString(), zone[0].Maghrib, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Maghrib = "Publishsed | " + zone[0].Maghrib;      
                    listBox8.Invoke((Action)(() => listBox8.Items.Add(message10minMaghrib)));
                    await ConnectMQTT(message10minMaghrib);                 
                }
                if (zone[0].Isyak == timeGMT8)
                {
                    await Message10minIsyak(zone[0].zoneNum.ToString(), zone[0].Isyak, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Isyak = "Publishsed | " + zone[0].Isyak;                 
                    listBox8.Invoke((Action)(() => listBox8.Items.Add(message10minIsyak)));
                    await ConnectMQTT(message10minIsyak);
                }
            }
        }
        private async Task Execute_MQtt_5minReminderZone8()
        {
            foreach (var zone in timeMinus5min8.Values.ToList())
            {
                if (zone[0].Subuh == timeGMT8)
                {
                    await Message5minSubuh(zone[0].zoneNum.ToString(), zone[0].Subuh, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Subuh = "Publishsed | " + zone[0].Subuh;       
                    listBox8.Invoke((Action)(() => listBox8.Items.Add(message5minSubuh)));
                    await ConnectMQTT(message5minSubuh);                   
                }
                if (zone[0].Zohor == timeGMT8)
                {
                    await Message5minZohor(zone[0].zoneNum.ToString(), zone[0].Zohor, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Zohor = "Publishsed | " + zone[0].Zohor; 
                    listBox8.Invoke((Action)(() => listBox8.Items.Add(message5minZohor)));
                    await ConnectMQTT(message5minZohor);
                }
                if (zone[0].Asar == timeGMT8)
                {
                    await Message5minAsar(zone[0].zoneNum.ToString(), zone[0].Asar, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Asar = "Publishsed | " + zone[0].Asar;              
                    listBox8.Invoke((Action)(() => listBox8.Items.Add(message5minAsar)));
                    await ConnectMQTT(message5minAsar);               
                }
                if (zone[0].Maghrib == timeGMT8)
                {
                    await Message5minMaghrib(zone[0].zoneNum.ToString(), zone[0].Maghrib, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Maghrib = "Publishsed | " + zone[0].Maghrib;             
                    listBox8.Invoke((Action)(() => listBox8.Items.Add(message5minMaghrib)));
                    await ConnectMQTT(message5minMaghrib);             
                }
                if (zone[0].Isyak == timeGMT8)
                {
                    await Message5minIsyak(zone[0].zoneNum.ToString(), zone[0].Isyak, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Isyak = "Publishsed | " + zone[0].Isyak;                
                    listBox8.Invoke((Action)(() => listBox8.Items.Add(message5minIsyak)));
                    await ConnectMQTT(message5minIsyak);            
                }
            }
        }
        private async Task Execute_MQtt_Zone8()
        {
            foreach (var zone in azanTimeMethods8.Values.ToList())
            {
                if (zone[0].Subuh == timeGMT8)
                {
                    await MessageMQTTSubuh(zone[0].zoneNum.ToString(), zone[0].Subuh, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Subuh = "Publishsed | " + zone[0].Subuh;                 
                    Invoke((Action)(() => listBox8.Items.Add(messageSubuh)));
                    await ConnectMQTT(messageSubuh);               
                }
                if (zone[0].Zohor == timeGMT8)
                {
                    await MessageMQTTZohor(zone[0].zoneNum.ToString(), zone[0].Zohor, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Zohor = "Publishsed | " + zone[0].Zohor;                
                    listBox8.Invoke((Action)(() => listBox8.Items.Add(messageZohor)));
                    await ConnectMQTT(messageZohor);        
                }
                if (zone[0].Asar == timeGMT8)
                {
                    await MessageMQTTAsar(zone[0].zoneNum.ToString(), zone[0].Asar, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Asar = "Publishsed | " + zone[0].Asar;                   
                    listBox8.Invoke((Action)(() => listBox8.Items.Add(messageAsar)));
                    await ConnectMQTT(messageAsar);              
                }
                if (zone[0].Maghrib == timeGMT8)
                {
                    await MessageMQTTMaghrib(zone[0].zoneNum.ToString(), zone[0].Maghrib, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Maghrib = "Publishsed | " + zone[0].Maghrib;                
                    listBox8.Invoke((Action)(() => listBox8.Items.Add(messageMaghrib)));
                    await ConnectMQTT(messageMaghrib);                 
                }
                if (zone[0].Isyak == timeGMT8)
                {
                    await MessageMQTTIsyak(zone[0].zoneNum.ToString(), zone[0].Isyak, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Isyak = "Publishsed | " + zone[0].Isyak;
                    listBox8.Invoke((Action)(() => listBox8.Items.Add(messageIsyak)));
                    await ConnectMQTT(messageIsyak);
                }
            }
        }

        private async Task Execute_MQtt_15minReminderZone7()
        {
            foreach (var zone in timeMinus15min7.Values.ToList())
            {
                if (zone[0].Subuh == timeGMT7)
                {
                    await Message15minSubuh(zone[0].zoneNum.ToString(), zone[0].Subuh, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Subuh = "Publishsed | " + zone[0].Subuh;               
                    listBox7.Invoke((Action)(() => listBox7.Items.Add(message15minSubuh)));
                    await ConnectMQTTGMT7(message15minSubuh);                   
                }
                if (zone[0].Zohor == timeGMT7)
                {
                    await Message15minZohor(zone[0].zoneNum.ToString(), zone[0].Zohor, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Zohor = "Publishsed | " + zone[0].Zohor;             
                    listBox7.Invoke((Action)(() => listBox7.Items.Add(message15minZohor)));
                    await ConnectMQTTGMT7(message15minZohor);             
                }
                if (zone[0].Asar == timeGMT7)
                {
                    await Message15minAsar(zone[0].zoneNum.ToString(), zone[0].Asar, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Asar = "Publishsed | " + zone[0].Asar;                
                    listBox7.Invoke((Action)(() => listBox7.Items.Add(message15minAsar)));
                    await ConnectMQTTGMT7(message15minAsar);                
                }
                if (zone[0].Maghrib == timeGMT7)
                {
                    await Message15minMaghrib(zone[0].zoneNum.ToString(), zone[0].Maghrib, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Maghrib = "Publishsed | " + zone[0].Maghrib;                 
                    listBox7.Invoke((Action)(() => listBox7.Items.Add(message15minMaghrib)));
                    await ConnectMQTTGMT7(message15minMaghrib);                 
                }
                if (zone[0].Isyak == timeGMT7)
                {
                    await Message15minIsyak(zone[0].zoneNum.ToString(), zone[0].Isyak, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Isyak = "Publishsed | " + zone[0].Isyak;                 
                    listBox7.Invoke((Action)(() => listBox7.Items.Add(message15minIsyak)));
                    await ConnectMQTTGMT7(message15minIsyak);             
                }
            }
        }
        private async Task Execute_MQtt_10minReminderZone7()
        {
            foreach (var zone in timeMinus10min7.Values.ToList())
            {
                if (zone[0].Subuh == timeGMT7)
                {
                    await Message10minSubuh(zone[0].zoneNum.ToString(), zone[0].Subuh, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Subuh = "Publishsed | " + zone[0].Subuh;
                    listBox7.Invoke((Action)(() => listBox7.Items.Add(message10minSubuh)));
                    await ConnectMQTTGMT7(message10minSubuh);

                }
                if (zone[0].Zohor == timeGMT7)
                {
                    await Message10minZohor(zone[0].zoneNum.ToString(), zone[0].Zohor, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Zohor = "Publishsed | " + zone[0].Zohor;
                    listBox7.Invoke((Action)(() => listBox7.Items.Add(message10minZohor)));
                    await ConnectMQTTGMT7(message10minZohor);

                }
                if (zone[0].Asar == timeGMT7)
                {
                    await Message10minAsar(zone[0].zoneNum.ToString(), zone[0].Asar, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Asar = "Publishsed | " + zone[0].Asar;
                    listBox7.Invoke((Action)(() => listBox7.Items.Add(message10minAsar)));
                    await ConnectMQTTGMT7(message10minAsar);

                }
                if (zone[0].Maghrib == timeGMT7)
                {
                    await Message10minMaghrib(zone[0].zoneNum.ToString(), zone[0].Maghrib, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Maghrib = "Publishsed | " + zone[0].Maghrib;
                    listBox7.Invoke((Action)(() => listBox7.Items.Add(message10minMaghrib)));
                    await ConnectMQTTGMT7(message10minMaghrib);

                }
                if (zone[0].Isyak == timeGMT7)
                {
                    await Message10minIsyak(zone[0].zoneNum.ToString(), zone[0].Isyak, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Isyak = "Publishsed | " + zone[0].Isyak;
                    listBox7.Invoke((Action)(() => listBox7.Items.Add(message10minIsyak)));
                    await ConnectMQTTGMT7(message10minIsyak);
                }
            }
        }
        private async Task Execute_MQtt_5minReminderZone7()
        {
            foreach (var zone in timeMinus5min7.Values.ToList())
            {
                if (zone[0].Subuh == timeGMT7)
                {
                    await Message5minSubuh(zone[0].zoneNum.ToString(), zone[0].Subuh, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Subuh = "Publishsed | " + zone[0].Subuh;               
                    listBox7.Invoke((Action)(() => listBox7.Items.Add(message5minSubuh)));
                    await ConnectMQTTGMT7(message5minSubuh);        
                }
                if (zone[0].Zohor == timeGMT7)
                {
                    await Message5minZohor(zone[0].zoneNum.ToString(), zone[0].Zohor, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Zohor = "Publishsed | " + zone[0].Zohor;                
                    listBox7.Invoke((Action)(() => listBox7.Items.Add(message5minZohor)));
                    await ConnectMQTTGMT7(message5minZohor);           
                }
                if (zone[0].Asar == timeGMT7)
                {
                    await Message5minAsar(zone[0].zoneNum.ToString(), zone[0].Asar, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Asar = "Publishsed | " + zone[0].Asar;                  
                    listBox7.Invoke((Action)(() => listBox7.Items.Add(message5minAsar)));
                    await ConnectMQTTGMT7(message5minAsar);             
                }
                if (zone[0].Maghrib == timeGMT7)
                {
                    await Message5minMaghrib(zone[0].zoneNum.ToString(), zone[0].Maghrib, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Maghrib = "Publishsed | " + zone[0].Maghrib;              
                    listBox7.Invoke((Action)(() => listBox7.Items.Add(message5minMaghrib)));
                    await ConnectMQTTGMT7(message5minMaghrib);                   
                }
                if (zone[0].Isyak == timeGMT7)
                {
                    await Message5minIsyak(zone[0].zoneNum.ToString(), zone[0].Isyak, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Isyak = "Publishsed | " + zone[0].Isyak;                
                    listBox7.Invoke((Action)(() => listBox7.Items.Add(message5minIsyak)));
                    await ConnectMQTTGMT7(message5minIsyak);                
                }
            }
        }
        private async Task Execute_MQtt_Zone7()
        {
            foreach (var zone in azanTimeMethods7.Values.ToList())
            {
                if (zone[0].Subuh == timeGMT7)
                {
                    await MessageMQTTSubuh(zone[0].zoneNum.ToString(), zone[0].Subuh, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Subuh = "Publishsed | " + zone[0].Subuh;                
                    listBox7.Invoke((Action)(() => listBox7.Items.Add(messageSubuh)));
                    await ConnectMQTTGMT7(messageSubuh);                
                }
               if (zone[0].Zohor == timeGMT7)
                {
                    await MessageMQTTZohor(zone[0].zoneNum.ToString(), zone[0].Zohor, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Zohor = "Publishsed | " + zone[0].Zohor;                 
                    listBox7.Invoke((Action)(() => listBox7.Items.Add(messageZohor)));
                    await ConnectMQTTGMT7(messageZohor);               
                }
                if (zone[0].Asar == timeGMT7)
                {
                    await MessageMQTTAsar(zone[0].zoneNum.ToString(), zone[0].Asar, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Asar = "Publishsed | " + zone[0].Asar;        
                    listBox7.Invoke((Action)(() => listBox7.Items.Add(messageAsar)));
                    await ConnectMQTTGMT7(messageAsar);           
                }
                if (zone[0].Maghrib == timeGMT7)
                {
                    await MessageMQTTMaghrib(zone[0].zoneNum.ToString(), zone[0].Maghrib, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Maghrib = "Publishsed | " + zone[0].Maghrib;             
                    listBox7.Invoke((Action)(() => listBox7.Items.Add(messageMaghrib)));
                    await ConnectMQTTGMT7(messageMaghrib);            
                }
                if (zone[0].Isyak == timeGMT7)
                {
                    await MessageMQTTIsyak(zone[0].zoneNum.ToString(), zone[0].Isyak, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Isyak = "Publishsed | " + zone[0].Isyak;                
                    listBox7.Invoke((Action)(() => listBox7.Items.Add(messageIsyak)));
                    await ConnectMQTTGMT7(messageIsyak);              
                }
            }
        }
        private async Task Execute_MQtt_15minReminderZone9()
        {
            foreach (var zone in timeMinus15min9.Values.ToList())
            {
                if (zone[0].Subuh == timeGMT9)
                {
                    await Message15minSubuh(zone[0].zoneNum.ToString(), zone[0].Subuh, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Subuh = "Publishsed | " + zone[0].Subuh;
                    listBox9.Invoke((Action)(() => listBox9.Items.Add(message15minSubuh)));
                    await ConnectMQTTGMT9(message15minSubuh);       
                }
                if (zone[0].Zohor == timeGMT9)
                {
                    await Message15minZohor(zone[0].zoneNum.ToString(), zone[0].Zohor, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Zohor = "Publishsed | " + zone[0].Zohor;          
                    listBox9.Invoke((Action)(() => listBox9.Items.Add(message15minZohor)));
                    await ConnectMQTTGMT9(message15minZohor);     
                }
                if (zone[0].Asar == timeGMT9)
                {
                    await Message15minAsar(zone[0].zoneNum.ToString(), zone[0].Asar, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Asar = "Publishsed | " + zone[0].Asar;             
                    listBox9.Invoke((Action)(() => listBox9.Items.Add(message15minAsar)));
                    await ConnectMQTTGMT9(message15minAsar);                 
                }
                if (zone[0].Maghrib == timeGMT9)
                {
                    await Message15minMaghrib(zone[0].zoneNum.ToString(), zone[0].Maghrib, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Maghrib = "Publishsed | " + zone[0].Maghrib;                
                    listBox9.Invoke((Action)(() => listBox9.Items.Add(message15minMaghrib)));
                    await ConnectMQTTGMT9(message15minMaghrib);
                }
                if (zone[0].Isyak == timeGMT9)
                {
                    await Message15minIsyak(zone[0].zoneNum.ToString(), zone[0].Isyak, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Isyak = "Publishsed | " + zone[0].Isyak;                
                    listBox9.Invoke((Action)(() => listBox9.Items.Add(message15minIsyak)));
                    await ConnectMQTTGMT9(message15minIsyak);                  
                }
            }
        }
        private async Task Execute_MQtt_10minReminderZone9()
        {
            foreach (var zone in timeMinus10min9.Values.ToList())
            {
                if (zone[0].Subuh == timeGMT9)
                {
                    await Message10minSubuh(zone[0].zoneNum.ToString(), zone[0].Subuh, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Subuh = "Publishsed | " + zone[0].Subuh;               
                    listBox9.Invoke((Action)(() => listBox9.Items.Add(message10minSubuh)));
                    await ConnectMQTTGMT9(message10minSubuh);              
                }
                if (zone[0].Zohor == timeGMT9)
                {
                    await Message10minZohor(zone[0].zoneNum.ToString(), zone[0].Zohor, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Zohor = "Publishsed | " + zone[0].Zohor;                 
                    listBox9.Invoke((Action)(() => listBox9.Items.Add(message10minZohor)));
                    await ConnectMQTTGMT9(message10minZohor);
                }
                if (zone[0].Asar == timeGMT9)
                {
                    await Message10minAsar(zone[0].zoneNum.ToString(), zone[0].Asar, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Asar = "Publishsed | " + zone[0].Asar;                
                    listBox9.Invoke((Action)(() => listBox9.Items.Add(message10minAsar)));
                    await ConnectMQTTGMT9(message10minAsar);                
                }
                if (zone[0].Maghrib == timeGMT9)
                {
                    await Message10minMaghrib(zone[0].zoneNum.ToString(), zone[0].Maghrib, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Maghrib = "Publishsed | " + zone[0].Maghrib;                 
                    listBox9.Invoke((Action)(() => listBox9.Items.Add(message10minMaghrib)));
                    await ConnectMQTTGMT9(message10minMaghrib);                 
                }
                if (zone[0].Isyak == timeGMT9)
                {
                    await Message10minIsyak(zone[0].zoneNum.ToString(), zone[0].Isyak, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Isyak = "Publishsed | " + zone[0].Isyak;                 
                    listBox9.Invoke((Action)(() => listBox9.Items.Add(message10minIsyak)));
                    await ConnectMQTT(message10minIsyak);                   
                }
            }
        }
        private async Task Execute_MQtt_5minReminderZone9()
        {
            foreach (var zone in timeMinus5min9.Values.ToList())
            {
                if (zone[0].Subuh == timeGMT9)
                {
                    await Message5minSubuh(zone[0].zoneNum.ToString(), zone[0].Subuh, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Subuh = "Publishsed | " + zone[0].Subuh;            
                    listBox9.Invoke((Action)(() => listBox9.Items.Add(message5minSubuh)));
                    await ConnectMQTTGMT9(message5minSubuh);                
                }
                if (zone[0].Zohor == timeGMT9)
                {
                    await Message5minZohor(zone[0].zoneNum.ToString(), zone[0].Zohor, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Zohor = "Publishsed | " + zone[0].Zohor;                
                    listBox9.Invoke((Action)(() => listBox9.Items.Add(message5minZohor)));
                    await ConnectMQTTGMT9(message5minZohor);
                }
                if (zone[0].Asar == timeGMT9)
                {
                    await Message5minAsar(zone[0].zoneNum.ToString(), zone[0].Asar, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Asar = "Publishsed | " + zone[0].Asar;                 
                    listBox9.Invoke((Action)(() => listBox9.Items.Add(message5minAsar)));
                    await ConnectMQTTGMT9(message5minAsar);              
                }
                if (zone[0].Maghrib == timeGMT9)
                {
                    await Message5minMaghrib(zone[0].zoneNum.ToString(), zone[0].Maghrib, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Maghrib = "Publishsed | " + zone[0].Maghrib;                 
                    listBox9.Invoke((Action)(() => listBox9.Items.Add(message5minMaghrib)));
                    await ConnectMQTTGMT9(message5minMaghrib);
                 
                }
                if (zone[0].Isyak == timeGMT9)
                {
                    await Message5minIsyak(zone[0].zoneNum.ToString(), zone[0].Isyak, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Isyak = "Publishsed | " + zone[0].Isyak;                
                    listBox9.Invoke((Action)(() => listBox9.Items.Add(message5minIsyak)));
                    await ConnectMQTTGMT9(message5minIsyak);                  
                }
            }
        }
        private async Task Execute_MQtt_Zone9()
        {
            foreach (var zone in azanTimeMethods9.Values.ToList())
            {
                if (zone[0].Subuh == timeGMT9)
                {
                    await MessageMQTTSubuh(zone[0].zoneNum.ToString(), zone[0].Subuh, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Subuh = "Publishsed | " + zone[0].Subuh;               
                    listBox9.Invoke((Action)(() => listBox9.Items.Add(messageSubuh)));
                    await ConnectMQTTGMT9(messageSubuh);            
                }
                if (zone[0].Zohor == timeGMT9)
                {
                    await MessageMQTTZohor(zone[0].zoneNum.ToString(), zone[0].Zohor, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Zohor = "Publishsed | " + zone[0].Zohor;                
                    listBox9.Invoke((Action)(() => listBox9.Items.Add(messageZohor)));
                    await ConnectMQTTGMT9(messageZohor);           
                }
                if (zone[0].Asar == timeGMT9)
                {
                    await MessageMQTTAsar(zone[0].zoneNum.ToString(), zone[0].Asar, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Asar = "Publishsed | " + zone[0].Asar;                  
                    listBox9.Invoke((Action)(() => listBox9.Items.Add(messageAsar)));
                    await ConnectMQTTGMT9(messageAsar);          
                }
                if (zone[0].Maghrib == timeGMT9)
                {
                    await MessageMQTTMaghrib(zone[0].zoneNum.ToString(), zone[0].Maghrib, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Maghrib = "Publishsed | " + zone[0].Maghrib;                 
                    listBox9.Invoke((Action)(() => listBox9.Items.Add(messageMaghrib)));
                    await ConnectMQTTGMT9(messageMaghrib);              
                }
                if (zone[0].Isyak == timeGMT9)
                {
                    await MessageMQTTIsyak(zone[0].zoneNum.ToString(), zone[0].Isyak, zone[0].TarikhMiladi, zone[0].NameZone);
                    zone[0].Isyak = "Publishsed | " + zone[0].Isyak;                
                    listBox9.Invoke((Action)(() => listBox9.Items.Add(messageIsyak)));
                    await ConnectMQTTGMT9(messageIsyak);
                }           
            }          
        }

        public  Task<string> MessageMQTTSubuh(string zonenum, string timeMethod, string date, string location)
        {
            messageSubuh = $"{zonenum},Subuh: {timeMethod} | {date},(Location:{location})";
            return Task.FromResult(messageSubuh);
        }
        public  Task<string> Message15minSubuh(string zonenum, string timeMethod, string date, string location)
        {
            message15minSubuh = $"{zonenum},15min before Subuh: {timeMethod} | {date},(Location:{location})";
            return Task.FromResult(message15minSubuh);
        }
        public  Task<string> Message5minSubuh(string zonenum, string timeMethod, string date, string location)
        {
            message5minSubuh = $"{zonenum},5min before Subuh: {timeMethod} | {date},(Location:{location})";
            return Task.FromResult(message5minSubuh);
        }
        public  Task<string> Message10minSubuh(string zonenum, string timeMethod, string date, string location)
        {
            message10minSubuh = $"{zonenum},10min before Subuh: {timeMethod} | {date},(Location:{location})";
            return Task.FromResult(message10minSubuh);
           
        }

        public  Task<string> MessageMQTTZohor(string zonenum, string timeMethod, string date, string location)
        {
            messageZohor = $"{zonenum},Zohor: {timeMethod} | {date},(Location:{location})";
            return Task.FromResult(messageZohor);
        }
        public  Task<string> Message15minZohor(string zonenum, string timeMethod, string date, string location)
        {
            message15minZohor = $"{zonenum},15min before Zohor: {timeMethod} | {date},(Location:{location})";
            return Task.FromResult(message15minZohor);
        }
        public  Task<string> Message5minZohor(string zonenum, string timeMethod, string date, string location)
        {
            message5minZohor = $"{zonenum},5min before Zohor: {timeMethod} | {date},(Location:{location})";
            return Task.FromResult(message5minZohor);
        }
        public  Task<string> Message10minZohor(string zonenum, string timeMethod, string date, string location)
        {
            message10minZohor = $"{zonenum},10min before Zohor: {timeMethod} | {date},(Location:{location})";
            return Task.FromResult(message10minZohor);
        }

        public  Task<string> MessageMQTTAsar(string zonenum, string timeMethod, string date, string location)
        {
            messageAsar = $"{zonenum},Asar: {timeMethod} | {date},(Location:{location})";
            return Task.FromResult(messageAsar);
        }
        public  Task<string> Message15minAsar(string zonenum, string timeMethod, string date, string location)
        {
            message15minAsar = $"{zonenum},15min before Asar: {timeMethod} | {date},(Location:{location})";
            return Task.FromResult(message15minAsar);
        }
        public  Task<string> Message5minAsar(string zonenum, string timeMethod, string date, string location)
        {
            message5minAsar = $"{zonenum},5min before Asar: {timeMethod} | {date},(Location:{location})";
            return Task.FromResult(message5minAsar);
        }
        public  Task<string> Message10minAsar(string zonenum, string timeMethod, string date, string location)
        {
            message10minAsar = $"{zonenum},10min before Asar: {timeMethod} | {date},(Location:{location})";
            return Task.FromResult(message10minAsar);
        }

        public  Task<string> MessageMQTTIsyak(string zonenum, string timeMethod, string date, string location)
        {
            messageIsyak = $"{zonenum},Isyak: {timeMethod} | {date},(Location:{location})";
            return Task.FromResult(messageAsar);
        }
        public  Task<string> Message15minIsyak(string zonenum, string timeMethod, string date, string location)
        {
            message15minIsyak = $"{zonenum},15min before Isyak: {timeMethod} | {date},(Location:{location})";
            return Task.FromResult(message15minIsyak);
        }
        public  Task<string> Message5minIsyak(string zonenum, string timeMethod, string date, string location)
        {
            message5minIsyak = $"{zonenum},5min before Isyak: {timeMethod} | {date},(Location:{location})";
            return Task.FromResult(message5minIsyak);
        }
        public  Task<string> Message10minIsyak(string zonenum, string timeMethod, string date, string location)
        {
            message10minIsyak = $"{zonenum},10min before Isyak: {timeMethod} | {date},(Location:{location})";
            return Task.FromResult(message10minIsyak);
        }

        public  Task<string> MessageMQTTMaghrib(string zonenum, string timeMethod, string date, string location)
        {
            messageMaghrib = $"{zonenum},Maghrib: {timeMethod} | {date},(Location:{location})";
            return Task.FromResult(messageMaghrib);
        }
        public  Task<string> Message15minMaghrib(string zonenum, string timeMethod, string date, string location)
        {
            message15minMaghrib = $"{zonenum},15min before Maghrib: {timeMethod} | {date},(Location:{location})";
            return Task.FromResult(message15minMaghrib);
        }
        public  Task<string> Message5minMaghrib(string zonenum, string timeMethod, string date, string location)
        {
            message5minMaghrib = $"{zonenum},5min before Maghrib: {timeMethod} | {date},(Location:{location})";
            return Task.FromResult(message5minMaghrib);
        }
        public  Task<string> Message10minMaghrib(string zonenum, string timeMethod, string date, string location)
        {
            message10minMaghrib = $"{zonenum},10min before Maghrib: {timeMethod} | {date},(Location:{location})";
            return Task.FromResult(message10minMaghrib);
        }

        static  async  Task PublishMessageAsync(string topic, string payload)
        {
            var Message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithExactlyOnceQoS()
                .WithRetainFlag()
                .Build();

            if (client.IsConnected)
            {
                await client.PublishAsync(Message);
                Console.WriteLine(Message.ConvertPayloadToString());            
            }
        }
        private async Task ConnectMQTT(string message)

        {
            try
            {
                var Message = message.Split(',' , '|');

                var factory = new MqttFactory();
                client = factory.CreateMqttClient();

                options = new MqttClientOptionsBuilder()
                      .WithClientId(Message[0])
                      .WithTcpServer("localhost", 1883)
                      .WithCleanSession()
                      .Build();

                client.UseConnectedHandler(e => {
                    Console.WriteLine("Connected successfully with MQTT Brokers.");
                });
                client.UseDisconnectedHandler(e => {
                    Console.WriteLine("Disconnected from MQTT Brokers.");
                });
                
                await client.ConnectAsync(options);
                await PublishMessageAsync(Message[0], Message[1]);
                await client.DisconnectAsync();          
            }
            catch (Exception e)
            {
               Console.WriteLine(e);
            }
           

        }    

        static  async Task PublishMessageAsyncGMT7(string topic, string payload)
        {
            var Message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithExactlyOnceQoS()
                .WithRetainFlag()
                .Build();

            if (client.IsConnected)
            {
                await client.PublishAsync(Message);
                Console.WriteLine(Message.ConvertPayloadToString());               
            }
        
        }
        private async Task ConnectMQTTGMT7(string message)

        {
            try
            {
                var Message = message.Split(',', '|');

                var factory = new MqttFactory();
                client = factory.CreateMqttClient();

                options = new MqttClientOptionsBuilder()
                      .WithClientId(Message[0])
                      .WithTcpServer("localhost", 1883)
                      .WithCleanSession()
                      .Build();

                client.UseConnectedHandler(e => {
                    Console.WriteLine("Connected successfully with MQTT Brokers.");
                });
                client.UseDisconnectedHandler(e => {
                    Console.WriteLine("Disconnected from MQTT Brokers.");
                });
                
                 await client.ConnectAsync(options);
                 await PublishMessageAsyncGMT7(Message[0], Message[1]);
                 await client.DisconnectAsync();
            }
            catch (Exception e)
            {
               Console.WriteLine(e);
            }

       }   

        static async Task PublishMessageAsyncGMT9(string topic, string payload)
        {
            var Message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithExactlyOnceQoS()
                .WithRetainFlag()
                .Build();

            if (client.IsConnected)
            {
                await client.PublishAsync(Message);
                Console.WriteLine(Message.ConvertPayloadToString());             
            }
 
        }
        private async Task ConnectMQTTGMT9(string message)

        {
            try
            {
                var Message = message.Split(',', '|');

                var factory = new MqttFactory();
                client = factory.CreateMqttClient();

                options = new MqttClientOptionsBuilder()
                      .WithClientId(Message[0])
                      .WithTcpServer("localhost", 1883)
                      .WithCleanSession()
                      .Build();

                client.UseConnectedHandler(e => {
                    Console.WriteLine("Connected successfully with MQTT Brokers.");
                });
                client.UseDisconnectedHandler(e => {
                    Console.WriteLine("Disconnected from MQTT Brokers.");
                });
                
                 await client.ConnectAsync(options);
                 await PublishMessageAsyncGMT9(Message[0], Message[1]);
                 await client.DisconnectAsync();                        
            }
            catch (Exception e)
            {
               Console.WriteLine(e);
            }

        }    
    }
}



