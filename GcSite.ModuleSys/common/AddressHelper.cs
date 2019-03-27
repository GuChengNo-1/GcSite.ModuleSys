using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GcSite.ModuleSys.common
{
    public class AddressHelper
    {
        public static List<string[]> AllCity()
        {
            List<string[]> allCityList = new List<string[]>();

            allCityList.Add(new string[] { "北京" });
            allCityList.Add(new string[] { "上海" });
            allCityList.Add(new string[] { "天津" });
            allCityList.Add(new string[] { "重庆" });


            allCityList.Add(new string[] { "哈尔滨", "齐齐哈尔", "牡丹江", "大庆", "伊春", "双鸭山", "鹤岗", "鸡西", "佳木斯", "七台河", "黑河", "绥化", "大兴安岭" });
            allCityList.Add(new string[] { "长春", "延边", "吉林", "白山", "白城", "四平", "松原", "辽源", "大安", "通化" });
            allCityList.Add(new string[] { "沈阳", "大连", "葫芦岛", "旅顺", "本溪", "抚顺", "铁岭", "辽阳", "营口", "阜新", "朝阳", "锦州", "丹东", "鞍山" });
            allCityList.Add(new string[] { "呼和浩特", "呼伦贝尔", "锡林浩特", "包头", "赤峰", "海拉尔", "乌海", "鄂尔多斯", "通辽" });

            allCityList.Add(new string[] { "石家庄", "唐山", "张家口", "廊坊", "邢台", "邯郸", "沧州", "衡水", "承德", "保定", "秦皇岛" });
            allCityList.Add(new string[] { "郑州", "开封", "洛阳", "平顶山", "焦作", "鹤壁", "新乡", "安阳", "濮阳", "许昌", "漯河", "三门峡", "南阳", "商丘", "信阳", "周口", "驻马店" });
            allCityList.Add(new string[] { "济南", "青岛", "淄博", "威海", "曲阜", "临沂", "烟台", "枣庄", "聊城", "济宁", "菏泽", "泰安", "日照", "东营", "德州", "滨州", "莱芜", "潍坊" });
            allCityList.Add(new string[] { "太原", "阳泉", "晋城", "晋中", "临汾", "运城", "长治", "朔州", "忻州", "大同", "吕梁" });

            allCityList.Add(new string[] { "南京", "苏州", "昆山", "南通", "太仓", "吴县", "徐州", "宜兴", "镇江", "淮安", "常熟", "盐城", "泰州", "无锡", "连云港", "扬州", "常州", "宿迁" });
            allCityList.Add(new string[] { "合肥", "巢湖", "蚌埠", "安庆", "六安", "滁州", "马鞍山", "阜阳", "宣城", "铜陵", "淮北", "芜湖", "毫州", "宿州", "淮南", "池州" });
            allCityList.Add(new string[] { "西安", "韩城", "安康", "汉中", "宝鸡", "咸阳", "榆林", "渭南", "商洛", "铜川", "延安" });
            allCityList.Add(new string[] { "银川", "固原", "中卫", "石嘴山", "吴忠" });

            allCityList.Add(new string[] { "兰州", "白银", "庆阳", "酒泉", "天水", "武威", "张掖", "甘南", "临夏", "平凉", "定西", "金昌" });
            allCityList.Add(new string[] { "西宁", "海北", "海西", "黄南", "果洛", "玉树", "海东", "海南" });
            allCityList.Add(new string[] { "武汉", "宜昌", "黄冈", "恩施", "荆州", "神农架", "十堰", "咸宁", "襄樊", "孝感", "随州", "黄石", "荆门", "鄂州" });
            allCityList.Add(new string[] { "长沙", "邵阳", "常德", "郴州", "吉首", "株洲", "娄底", "湘潭", "益阳", "永州", "岳阳", "衡阳", "怀化", "韶山", "张家界" });

            allCityList.Add(new string[] { "杭州", "湖州", "金华", "宁波", "丽水", "绍兴", "雁荡山", "衢州", "嘉兴", "台州", "舟山", "温州" });
            allCityList.Add(new string[] { "南昌", "萍乡", "九江", "上饶", "抚州", "吉安", "鹰潭", "宜春", "新余", "景德镇", "赣州" });
            allCityList.Add(new string[] { "福州", "厦门", "龙岩", "南平", "宁德", "莆田", "泉州", "三明", "漳州" });
            allCityList.Add(new string[] { "贵阳", "安顺", "赤水", "遵义", "铜仁", "六盘水", "毕节", "凯里", "都匀" });

            allCityList.Add(new string[] { "成都", "泸州", "内江", "凉山", "阿坝", "巴中", "广元", "乐山", "绵阳", "德阳", "攀枝花", "雅安", "宜宾", "自贡", "甘孜州", "达州", "资阳", "广安", "遂宁", "眉山", "南充" });
            allCityList.Add(new string[] { "广州", "深圳", "潮州", "韶关", "湛江", "惠州", "清远", "东莞", "江门", "茂名", "肇庆", "汕尾", "河源", "揭阳", "梅州", "中山", "德庆", "阳江", "云浮", "珠海", "汕头", "佛山" });
            allCityList.Add(new string[] { "南宁", "桂林", "阳朔", "柳州", "梧州", "玉林", "桂平", "贺州", "钦州", "贵港", "防城港", "百色", "北海", "河池", "来宾", "崇左" });
            allCityList.Add(new string[] { "昆明", "保山", "楚雄", "德宏", "红河", "临沧", "怒江", "曲靖", "思茅", "文山", "玉溪", "昭通", "丽江", "大理" });

            allCityList.Add(new string[] { "海口", "三亚", "儋州", "琼山", "通什", "文昌" });
            allCityList.Add(new string[] { "乌鲁木齐", "阿勒泰", "阿克苏", "昌吉", "哈密", "和田", "喀什", "克拉玛依", "石河子", "塔城", "库尔勒", "吐鲁番", "伊宁" });

            allCityList.Add(new string[] { "拉萨", "昌都地区", "山南地区", "阿里地区", "那曲地区", "林芝地区", "日喀则地区" });
            allCityList.Add(new string[] { "香港" });
            allCityList.Add(new string[] { "澳门" });
            allCityList.Add(new string[] { "台湾" });


            return allCityList;
        }
        public static List<string> ProvinceList()
        {
            List<string> list = new List<string>();

            list.Add("北京");
            list.Add("上海");
            list.Add("天津");
            list.Add("重庆");

            list.Add("黑龙江");
            list.Add("吉林");
            list.Add("辽宁");
            list.Add("内蒙古");

            list.Add("河北");
            list.Add("河南");
            list.Add("山东");
            list.Add("山西");

            list.Add("江苏");
            list.Add("安徽");
            list.Add("陕西");
            list.Add("宁夏");

            list.Add("甘肃");
            list.Add("青海");
            list.Add("湖北");
            list.Add("湖南");

            list.Add("浙江");
            list.Add("江西");
            list.Add("福建");
            list.Add("贵州");

            list.Add("四川");
            list.Add("广东");
            list.Add("广西");
            list.Add("云南");

            list.Add("海南");
            list.Add("新疆");
            list.Add("西藏");

            list.Add("香港");
            list.Add("澳门");
            list.Add("台湾");
            return list;
        }
        public static string FindObjectProvince(string cityName)
        {
            if (cityName.Contains("市"))
            {
                int index = cityName.IndexOf("市");
                cityName = cityName.Substring(0, index);

            }
            for (int i = 0; i < AllCity().Count(); i++)
            {
                for (int j = 0; j < AllCity().ElementAt(i).Length; j++)
                {
                    if (AllCity().ElementAt(i)[j].Equals(cityName))
                    {
                        string provinceName = ProvinceList().ElementAt(i);
                        return provinceName;
                    }
                }
            }
            return "查询失败";
        }
    }
}