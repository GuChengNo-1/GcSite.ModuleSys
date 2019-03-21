
var myChart = echarts.init(document.getElementById('cha1'));
// 指定图表的配置项和数据
option = {
    tooltip: {
        trigger: 'item',
        formatter: "{a} <br/>{b}: {c} ({d}%)"
    },
    legend: {
        orient: 'vertical',
        x: 'left',
        data:['新客户','老客户']
    },
    series: [
        {
            name:'访问来源',
            type:'pie',
            radius: ['50%', '70%'],
            avoidLabelOverlap: false,
            label: {
                normal: {
                    show: false,
                    position: 'center'
                },
                emphasis: {
                    show: true,
                    textStyle: {
                        fontSize: '14',
                        fontWeight: 'bold'
                    }
                }
            },
            labelLine: {
                normal: {
                    show: false
                }
            },
            data:[
                {value:335, name:'新客户'},
                {value:30, name:'老客户'},
            ]
        }
    ]
};
myChart.setOption(option);




var myChart = echarts.init(document.getElementById('cha2'));

option = {

    tooltip: {
        trigger: 'axis'
    },
    legend: {
        data:['新客户','老客户']
    },
    grid: {
        left: '3%',
        right: '4%',
        bottom: '3%',
        containLabel: true
    },
    toolbox: {
        feature: {
            saveAsImage: {}
        }
    },
    xAxis: {
        type: 'category',
        boundaryGap: false,
        data: ['2019/02/01','2019/03/01','2019/04/01']
    },
    yAxis: {
        type: 'value'
    },
    series: [
        {
            name:'新客户',
            type:'line',
            stack: '总量',
            data:[10, 132, 101]
        },
        {
            name:'老客户',
            type:'line',
            stack: '总量',
            data:[220, 182, 191]
        },
    ]
};

myChart.setOption(option);



var myChart = echarts.init(document.getElementById('cha3'));


option = {
    tooltip: {
        trigger: 'axis'
    },
    legend: {
        data:['新客户','老客户']
    },
    grid: {
        left: '3%',
        right: '4%',
        bottom: '3%',
        containLabel: true
    },
    toolbox: {
        feature: {
            saveAsImage: {}
        }
    },
    xAxis: {
        type: 'category',
        boundaryGap: false,
        data: ['2019/02/01','2019/03/01','2019/04/01']
    },
    yAxis: {
        type: 'value'
    },
    series: [
        {
            name:'新客户',
            type:'line',
            stack: '总量',
            data:[1, 132, 300]
        },
        {
            name:'老客户',
            type:'line',
            stack: '总量',
            data:[400, 182, 191]
        },
    ]
};

myChart.setOption(option);




var myChart = echarts.init(document.getElementById('cha4'));


option = {
    tooltip: {
        trigger: 'axis'
    },
    legend: {
        data:['新客户','老客户']
    },
    grid: {
        left: '3%',
        right: '4%',
        bottom: '3%',
        containLabel: true
    },
    toolbox: {
        feature: {
            saveAsImage: {}
        }
    },
    xAxis: {
        type: 'category',
        boundaryGap: false,
        data: ['2019/02/01','2019/03/01','2019/04/01']
    },
    yAxis: {
        type: 'category',
        data: ['00:05:00','00:10:00','00:15:00']
    },
    series: [
        {
            name:'新客户',
            type:'line',
            data:[0,0,2]
        },
        {
            name:'老客户',
            type:'line',
            data:[1,1,3]
        },
    ]
};

myChart.setOption(option);




var myChart = echarts.init(document.getElementById('cha5'));


option = {
    tooltip: {
        trigger: 'axis'
    },
    legend: {
        data:['新客户','老客户']
    },
    grid: {
        left: '3%',
        right: '4%',
        bottom: '3%',
        containLabel: true
    },
    xAxis: {
        type: 'category',
        boundaryGap: false,
        data: ['2019/02/01','2019/03/01','2019/04/01']
    },
    yAxis: {
        type: 'value',
        axisLabel: {
            formatter: '{value} %'
        }
    },
    series: [
        {
            name:'新客户',
            type:'line',
            stack: '总量',
            data:[1, 1, 100]
        },
        {
            name:'老客户',
            type:'line',
            stack: '总量',
            data:[100, 1, 6]
        },
    ]

};

myChart.setOption(option);



var myChart = echarts.init(document.getElementById('cha6'));


option = {
    tooltip: {
        trigger: 'axis'
    },
    legend: {
        data:['新客户','老客户']
    },
    grid: {
        left: '3%',
        right: '4%',
        bottom: '3%',
        containLabel: true
    },
    xAxis: {
        type: 'category',
        boundaryGap: false,
        data: ['2019/02/01','2019/03/01','2019/04/01']
    },
    yAxis: {
        type: 'value',
        axisLabel: {
            formatter: '{value} 页'
        }
    },
    series: [
        {
            name:'新客户',
            type:'line',
            stack: '总量',
            data:[1, 1, 5]
        },
        {
            name:'老客户',
            type:'line',
            stack: '总量',
            data:[1, 1, 4]
        },
    ]

};

myChart.setOption(option);



var myChart = echarts.init(document.getElementById('cha7'));
option = {
    tooltip: {
        trigger: 'axis',
        axisPointer: {
            type: 'shadow'
        }
    },
    legend: {
        orient: 'vertical',
        data:['老客户','新客户']
    },
    grid: {
        left: '3%',
        right: '4%',
        bottom: '3%',
        containLabel: true
    },
    yAxis: {
        type: 'value',
    },
    xAxis: {
        type: 'category',
        data: ['直接访问','搜索引擎','外部链接','自定义来源']
    },
    series: [
        {
            name: '新客户',
            type: 'bar',
            data: [100, 200, 290, 104]
        },
        {
            name: '老客户',
            type: 'bar',
            data: [193, 234, 31, 121]
        }
    ]
};

myChart.setOption(option);


var myChart = echarts.init(document.getElementById('aaa'));
function randomData() {
    return Math.round(Math.random()*1000);
}
// 指定图表的配置项和数据
//数据纯属虚构
option = {
    tooltip: {
        trigger: 'item'
    },
    legend: {
        orient: 'vertical',
        left: 'left',
        data:['老顾客','新顾客']
    },
    visualMap: {
        min: 0,
        max: 2500,
        left: 'left',
        top: 'bottom',
        text: ['高','低'],           // 文本，默认为数值文本
        calculable: true
    },
    toolbox: {
        show: true,
        orient: 'vertical',
        left: 'right',
        top: 'center',
        feature: {
            dataView: {readOnly: false},
            restore: {},
            saveAsImage: {}
        }
    },
    series: [
        {
            name: '老顾客',
            type: 'map',
            mapType: 'china',
            roam: false,
            label: {
                normal: {
                    show: true
                },
                emphasis: {
                    show: true
                }
            },
            data:[
                {name: '北京',value: randomData() },
                {name: '天津',value: randomData() },
                {name: '上海',value: randomData() },
                {name: '重庆',value: randomData() },
                {name: '河北',value: randomData() },
                {name: '河南',value: randomData() },
                {name: '云南',value: randomData() },
                {name: '辽宁',value: randomData() },
                {name: '黑龙江',value: randomData() },
                {name: '湖南',value: randomData() },
                {name: '安徽',value: randomData() },
                {name: '山东',value: randomData() },
                {name: '新疆',value: randomData() },
                {name: '江苏',value: randomData() },
                {name: '浙江',value: randomData() },
                {name: '江西',value: randomData() },
                {name: '湖北',value: randomData() },
                {name: '广西',value: randomData() },
                {name: '甘肃',value: randomData() },
                {name: '山西',value: randomData() },
                {name: '内蒙古',value: randomData() },
                {name: '陕西',value: randomData() },
                {name: '吉林',value: randomData() },
                {name: '福建',value: randomData() },
                {name: '贵州',value: randomData() },
                {name: '广东',value: randomData() },
                {name: '青海',value: randomData() },
                {name: '西藏',value: randomData() },
                {name: '四川',value: randomData() },
                {name: '宁夏',value: randomData() },
                {name: '海南',value: randomData() },
                {name: '台湾',value: randomData() },
                {name: '香港',value: randomData() },
                {name: '澳门',value: randomData() }
            ]
        },
        {
            name: '新顾客',
            type: 'map',
            mapType: 'china',
            label: {
                normal: {
                    show: true
                },
                emphasis: {
                    show: true
                }
            },
            data:[
                {name: '北京',value: randomData() },
                {name: '天津',value: randomData() },
                {name: '上海',value: randomData() },
                {name: '重庆',value: randomData() },
                {name: '河北',value: randomData() },
                {name: '安徽',value: randomData() },
                {name: '新疆',value: randomData() },
                {name: '浙江',value: randomData() },
                {name: '江西',value: randomData() },
                {name: '山西',value: randomData() },
                {name: '内蒙古',value: randomData() },
                {name: '吉林',value: randomData() },
                {name: '福建',value: randomData() },
                {name: '广东',value: randomData() },
                {name: '西藏',value: randomData() },
                {name: '四川',value: randomData() },
                {name: '宁夏',value: randomData() },
                {name: '香港',value: randomData() },
                {name: '澳门',value: randomData() }
            ]
        },
    ]
};
myChart.setOption(option);
