
$.extend($.fn.dataTable.defaults, {
    dom: 'Bfrtip',
    deferRender: true,
    lengthMenu: [[10, 25, 50, -1], [10, 25, 50, "All"]],
    scrollX: true,
    scrollCollapse: true,
    fixedColumns: true,
    scrollY: "25em",
    pagingType: "simple_numbers",
    buttons: [
        {
            extend: 'pageLength',
            titleAttr: 'pageLength',
            className: 'btn-default',
            backgroundClassName: 'test',
        },
        {
            extend: 'colvis',
            titleAttr: 'colvis',
            className: 'btn-default',
            backgroundClassName: 'test',
            text: '<i class="fa fa-columns"></i> Show/Hide Columns',
        },
        {
            extend: 'csv',
            titleAttr: 'Export to Excel',
            className: 'btn-default',
            text: '<i class="fa fa-table"></i> Export To Excel',
        },
    ],
} );
function detectmob() {
    var check = false;
    (function(a){if(/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino/i.test(a)||/1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(a.substr(0,4))) check = true;})(navigator.userAgent||navigator.vendor||window.opera);
    return check;
}

function siteFilter(table, colNum) { 
    $('.site-check').on('ifChanged', function (event) {
        var searchRegex = '';
        $('.site-check').each(function (index, value) {
            if (value.checked) {
                searchRegex += '^(' + value.dataset.siteName + ')$|';
            }
        });
        searchRegex = searchRegex.substring(0, searchRegex.length - 1);
        if (searchRegex === '')
            searchRegex = 'a^'; // match nothing
        table
            .columns(colNum) // column number of siteid
            .search(searchRegex, true)
            .draw();
    });
}

const getApi = (settings) => new $.fn.dataTable.Api(settings);
const getTable = (settings) => getApi(settings).table();
const getTableId = (settings) => getTable(settings).table().node().id;

$(document).on('preInit.dt', function (e, settings) {
    let pageLength = JSON.parse(localStorage.getItem(getTableId(settings) + '-pg-len-def.dt'));
    if (pageLength) {
        getTable(settings).page.len(pageLength);
    }
});

$(document).on('init.dt', function (e, settings) {
    let table = getTable(settings);
    let node = table.node();
    $(node).on('preXhr.dt', (e, settings, data) => {
        $(node.closest('.ibox-content')).toggleClass('sk-loading');
    });
    $(node).on('xhr.dt', (e, settings, data) => {
        $(node.closest('.ibox-content')).toggleClass('sk-loading');
    });
    if ($(node).closest('.ibox-content').hasClass('sk-loading')) {
        $(node.closest('.ibox-content')).toggleClass('sk-loading');
    }
    $('.dataTables_filter').addClass('pull-right');
    if (detectmob()) {        
        table.fixedHeader.disable();
    }
    window.mobilecheck = function() {
        var check = false;
        (function(a){if(/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino/i.test(a)||/1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(a.substr(0,4))) check = true;})(navigator.userAgent||navigator.vendor||window.opera);
        return check;
    };
    $(node).closest('.hide').removeClass('hide');
    table.columns().header().each(function (value, index) {        
        if (value.dataset.colVisible === "false")
            this.table().columns(index).visible(false);
    });

    let reduceColumns = (array, value) => {
        return array.reduce(function (a, e, i) {
            if (e === value)
                a.push(i);
            return a;
        }, []);
    };
    let id = table.table().node().id;
    let columns = JSON.parse(localStorage.getItem(id + '-col-user-def.dt'));

    if (columns) {
        let visibleColumns = reduceColumns(columns, true);
        table.columns(visibleColumns).visible(true);

        let hiddenColumns = reduceColumns(columns, false);
        table.columns(hiddenColumns).visible(false);
    }

    $(node).on('column-visibility.dt', function (e, settings, column, state) {
        let id = this.id;
        let columnVisibility = $(this).DataTable()
            .columns()
            .visible()
            .toArray();
        localStorage.setItem(id + '-col-user-def.dt', JSON.stringify(columnVisibility));
    });

    $(node).on('length.dt', function (e, settings, len) {
        let id = this.id;
        localStorage.setItem(id + '-pg-len-def.dt', JSON.stringify(len))
    });

    $.fn.dataTable.ext.errMode = function (settings, helpPage, message) {
        console.log(message);
    };
    table.draw();
});