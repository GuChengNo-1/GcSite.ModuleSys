$("#event-monitor-advanced-filter").click(function () {
    // $(this).parent("div").next("div").toggle();
    $(this).parents(".control-bar-wrapper").next("div").toggle();
    $(this).toggleClass("up");
    var Word = $(this).text()
    if ( Word === "收起筛选") {
        $(this).html("高级筛选");
    }
    if ( Word === "高级筛选") {
        $(this).html("收起筛选");
    }
});
$(".table-indicator-custom-btn").click(function () {
    $(this).next("div").toggle();
    $(".btn-focused").click(function () {
        $(".table-indicator").hide();
    });
});
$(".group input").click(function () {
    $(this).prev("div").toggleClass("checked");
});
$(".group li").click(function () {
    $(this).addClass("cur").siblings("li").removeClass("cur");
});

$("#compare-date").click(function () {
    $(this).parent("label").next("span").toggleClass("hide");
    var ht = $(this).next("span").html();
    if (ht === "对比时间段"){
        $(this).next("span").html("对比");
    } else {
        $(this).next("span").html("对比时间段");
    }
});

laydate.render({
    elem: '#date-select'
    ,type: 'datetime'
    ,range: '-'
    ,format: 'yyyy/M/d'
});
laydate.render({
    elem: '#compare-date-select'
    ,type: 'datetime'
    ,range: '-'
    ,format: 'yyyy/M/d'
});

$(".flash-indicator-wrapper").click(function () {
    $(this).addClass("btn-selected");
    $("#flash-option-container").toggle();
});

// che();
//     function che() {
//         $("#flash-option-container").find("input[type=checkbox]").bind("click",function () {
//
//         })
// }
$("#toggle-flash-btn").click(function () {
    $("#toggle-flash-btn").toggleClass("close");
    $("#flash-indicator").toggleClass("toggleable-hidden");
    $(this).parents(".fold").prev(".to-toggle").toggleClass("toggleable-hidden unvisible");
    $(".flash-text-container").children("span").toggleClass("toggleable-hidden");
})