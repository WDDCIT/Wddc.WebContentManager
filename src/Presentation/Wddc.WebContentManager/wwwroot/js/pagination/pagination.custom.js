function handleResponse(response) {
    if (response.ResultStatus === 1) // success
        successMessage(response.Message);
    else if (response.ResultStatus == 5) //warning
        warningMessage(response.Message)
    else if (response.ResultStatus == 10)
        errorMessage(response.Message)
}
function getCheckBox(id, name) {

    /* ADD CHECBKOX */
    var checkBoxCell = document.createElement('div');
    checkBoxCell.className += 'bs-ObjectList-cell';
    var label = document.createElement('label');
    label.className += 'bs-Checkbox';

    var input1 = document.createElement('input');
    input1.className += 'bs-Checkbox-source';
    input1.checked = true;
    input1.value = true;
    input1.type = 'checkbox';
    input1.name = name + '.Selected'; //'ReceivedParcels[' + i + '].Selected'
    input1.id = id; // data.Results[i].RouteId;
    label.appendChild(input1);

    var input2 = document.createElement('input');
    input2.name = name + '.Selected' //'ReceivedParcels[' + i + '].Selected';
    input2.type = 'hidden';
    input2.value = false;
    label.appendChild(input2);

    var span = document.createElement('span');
    span.className += 'bs-Checkbox-box';
    label.appendChild(span);
    checkBoxCell.appendChild(label);
    return checkBoxCell;
}
function setupPagination(id, listid, datasource, headers, pageSize) {
    var element = document.getElementById(id);
    $(element).pagination({
        dataSource: datasource,
        locator: 'Results',
        className: 'text-center',
        ulClassName: 'pagination',
        pageSize: pageSize,
        showNavigator: true,
        formatNavigator: '<%= totalNumber %> entries',
        ajax: {
            beforeSend: function () {
                $('#' + listid).html(getLoadingScreenHtmlWDDC());
            }
        },
        totalNumberLocator: function (response) {
            return response.TotalItemCount;
        },
        callback: function (data, pagination) {

            if (data.length > 0) {
                var html = generateList(headers, data);
                $('#' + listid).html(html);
            }
            else {
                $(element).pagination('hide');

                var cell = document.createElement('div');
                cell.className += 'text-center';
                cell.style = 'padding:15px;';
                cell.innerHTML = 'Log: No results found';
                $('#' + listid).html(cell);
            }
        }
    })
}

function getLoadingScreenHtml() {
    return `<div>
                <div class="load-partial">
                    <div class="loading-icon">
                        <svg width="84px" height="84px" viewBox="0 0 84 84" version="1.1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink">
                            <circle class="border" cx="42" cy="42" r="40" stroke-linecap="round" stroke-width="4" stroke="#000" fill="none"></circle>
                            <path class="checkmark" stroke-linecap="round" stroke-linejoin="round" d="M23.375 42.5488281 36.8840688 56.0578969 64.891932 28.0500338" stroke-width="4" stroke="#000" fill="none"></path>
                        </svg>
                    </div>
                    <div class="loading-text" style="text-align:center">Getting results from server</div>
                </div>
            </div>`
}

function getLoadingScreenHtmlWDDC() {
    return `
            <div style="text-align: center;" >
                <button class="buttonload">
                    <i class="fa fa-spinner fa-spin fa-lg"></i>&nbsp;&nbsp;Getting results from server
                </button>
            </div>`
}

function getCell(innerHTML) {
    var cell = document.createElement('div');
    cell.className += "bs-ObjectList-cell";
    cell.innerHTML = innerHTML;
    return cell;
}

function generateList(headers, data) {
    var list = document.createElement('div');
    list.className += 'bs-ObjectList-rows';

    var header = document.createElement('header');
    header.className += "bs-ObjectList-row bs-ObjectList-row--header";

    var properties = [];
    headers.forEach(function (element) {
        header.appendChild(getCell(element));
    });

    list.appendChild(header);

    //$('.bs-ObjectList-row--transfer').remove();

    for (var i = 0; i < data.length; i++) {

        // Create the list item:
        var row = document.createElement('div');
        row.className += 'bs-ObjectList-row bs-ObjectList-row--transfer IconParent comment-row';

        for (var key in data[i]) {
            var value = data[i][key];
            row.appendChild(getCell(data[i][key]));
        }

        // Add it to the list:
        list.appendChild(row);
    }

    // Finally, return the constructed list:
    return list;
}