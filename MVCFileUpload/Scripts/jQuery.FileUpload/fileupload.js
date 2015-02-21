/*jslint unparam: true, regexp: true */
/*global window, $ */
$(function () {
    'use strict';
    var apiUrl = parentUrl + 'api/fileutils',
        startButton = $('<button/>')
            .addClass('btn btn-xs btn-primary')
            .attr('title', 'upload')
            .on('click', function () {
                var $this = $(this),
                    data = $this.data();
                $this
                    .off('click')
                    .attr('title', 'abort')
                    .on('click', function () {
                        $this.remove();
                        data.abort();
                    })
                    .removeClass('btn-primary')
                    .addClass('btn-warning')
                    .find('#iconBtn')
                        .removeClass('glyphicon-play-circle')
                        .addClass('glyphicon-remove-circle');               
                data.submit().always(function () {
                    $this.remove();
                });
            })
            .append($('<span/>')
                .attr('id', 'iconBtn')
                .addClass('glyphicon glyphicon-play-circle')),
        removeButton = $('<button/>')
            .addClass('btn btn-xs btn-danger')
            .attr('title', 'delete')
            .on('click', function () {
                var $this = $(this),
                    data = $this.data();
                
                deleteFile(data, apiUrl);
                data.files[0].error = 'deleted';
                $this.remove();
            })
            .append($('<span/>')
                .attr('id', 'iconDel')
                .addClass('glyphicon glyphicon-trash')),
        fileProgress = $('<div/>')
            .addClass('progress')
            .append($('<div/>')
                .addClass('progress-bar progress-bar-success'));
            
    $('#fileupload').fileupload({
        headers: {
            'RequestVerificationToken': RequestVerificationToken
        },
        url: apiUrl + '/upload',
        dataType: 'json',
        autoUpload: false,
        acceptFileTypes: /(\.|\/)(gif|jpe?g|png|doc?x|doc|rtf|txt|pdf|zip)$/i,
        maxFileSize: 5000000, // 5 MB
        maxChunkSize: 4096, // in bytes
        // Enable image resizing, except for Android and Opera,
        // which actually support image resizing, but fail to
        // send Blob objects via XHR requests:
        disableImageResize: /Android(?!.*Chrome)|Opera/
            .test(window.navigator.userAgent),
        previewMaxWidth: 50,
        previewMaxHeight: 50,
        previewCrop: true
    }).on('fileuploadadd', function (e, data) {
        $('#fileListHolder').removeClass('hide');

        data.guId = guid();
        data.uId =  $('<div/>').uniqueId().attr('id');
        data.context = $('<tr/>')
            .attr('id', 'file-' + data.uId)
            .appendTo('#fileList');

        $.each(data.files, function (index, file) {
            fileCount++;
            var tdFileName = $('<td/>')
                    .append($('<span/>')
                        .attr('id', 'name-' + data.uId)
                        .text(file.name)),
                tdContentType = $('<td/>')
                    .append($('<span/>')
                        .text(file.type)),
                tdContentLength = $('<td/>')
                    .addClass('text-right')
                    .append($('<span/>')
                        .text(formatFileSize(file.size))),
                tdStatus = $('<td/>')
                    .attr('id', 'status-' + data.uId)
                    .append(fileProgress
                        .clone(true)),
                tdAction = $('<td/>')
                    .append(startButton
                        .clone(true)
                        .data(data)
                        .attr('id', 'start-' + data.uId))
                    .append(removeButton
                        .clone(true)
                        .data(data)
                        .attr('id', 'remove-' + data.uId));

            tdContentType.appendTo(data.context);
            tdFileName.appendTo(data.context);
            tdContentLength.appendTo(data.context);
            tdStatus.appendTo(data.context);
            tdAction.appendTo(data.context);
        });
        $("#fileCount").text(fileCount);
        $('#btnStartUpload').on('click', function () {
            var file = data.files[0];

            if (!file.error) {
                $(this).prop('disabled', true)
                data.context
                    .find('#start-' + data.uId)
                    .off('click')
                    .on('click', function () {
                        $(this).remove();
                        data.abort();
                    })
                        .removeClass('btn-primary')
                        .addClass('btn-warning')
                        .find('#iconBtn')
                            .removeClass('glyphicon-play-circle')
                            .addClass('glyphicon-remove-circle');
                data.submit();
            }
        }).prop('disabled', false);
    }).on('fileuploadprocessalways', function (e, data) {
        var index = data.index,
            file = data.files[index];

        if (file.error) {
            data.context
                .find('#status-' + data.uId)
                .append($('<span class="text-danger"/>')
                    .css('font-size', 'x-small')
                    .text(file.error));

            data.context
                .find('#start-' + data.uId)
                .off('click')
                .remove();

            data.context
                .find('.progress')
                .remove();
        }
    }).on('fileuploadprogress', function (e, data) {
        var progress = parseInt(formatPercentage(data.loaded / data.total));

        data.context.find('.progress-bar')
            .css('width', progress + '%')
            .text(progress >= 100 ? 'Uploaded' : progress + '%');
    }).on('fileuploadprogressall', function (e, data) {
        var progress = parseInt(formatPercentage(data.loaded / data.total));

        $('#progress .progress-bar')
            .css('width', progress + '%')
            .text(progress + '% @ ' + formatBitrate(data.bitrate));
    }).on('fileuploaddone', function (e, data) {
        $('#btnStartUpload').off('click');
        $.each(data.result.files, function (index, file) {
            if (file.uid) {
                var link = $('<a>')
                .prop('href',  apiUrl + '/download?id=' + file.uid + '&name=' + file.name);

                data.context
                    .find('#name-' + data.uId)
                    .wrap(link);

                data.context
                    .find('#remove-' + data.uId)
                    .data('uploaded', true);
            } else if (file.error) {
                data.context
                .find('#status-' + data.uId)
                    .append($('<span class="text-danger"/>')
                        .css('font-size', 'x-small')
                        .text(data.result.error));
            }
        });
        
        data.context
            .find('#start-' + data.uId)
                .off('click')
                .remove();
        totalUploaded++;
    }).on('fileuploadfail', function (e, data) {
        $.each(data.files, function (index) {
            data.context
                .find('#status-' + data.uId)
                .append($('<span class="text-danger"/>')
                    .css('font-size', 'x-small')
                    .text('File upload failed.'))
                .find('.progress')
                .remove();
            // data.jqXHR.statusText
        });
    }).on('fileuploadsubmit', function (e, data) {
        // make sure each submit is unique
        data.formData = { uid: data.guId };

        // initial call 
        $.ajax({
            headers: {
                'RequestVerificationToken': RequestVerificationToken
            },
            dataType: "json",
            url: apiUrl + "/upload",
            data: { uid: "init" },
            type: "POST",
            async: false,
            success: function (response) {
                //alert(response.name);
            }
        });
    }).prop('disabled', !$.support.fileInput)
        .parent().addClass($.support.fileInput ? undefined : 'disabled');
});

var fileCount = 0;
var totalUploaded = 0;
// delete a file
var deleteFile = function (data, apiUrl) {
    if (data.uploaded){
        $.ajax({
            headers: {
                'RequestVerificationToken': RequestVerificationToken
            },
            dataType: "json",
            url: apiUrl + "/delete",
            data: { uid: data.guId },
            type: "DELETE",
            success: function (response) {
                // do something here
            },
            error: function (xmlHttpRequest, textStatus, errorThrown) {
                if (window.console != undefined) {
                    window.console.log(textStatus);
                    window.console.log(errorThrown);
                }
            }
        });
    }

    // remove the item from the list
    data.context.remove();

    fileCount--;

    $("#fileCount").text(fileCount);

    var progress = formatPercentage(fileCount / totalUploaded)

    if (fileCount == 0) {
        $('#fileListHolder').addClass('hide');
        totalUploaded = 0;
        $('#btnStartUpload')
            .off('click')
            .prop('disabled', true);
    }

    $('#progress .progress-bar')
        .css('width', progress === "NaN" ? '0' : progress + '%')
        .text(progress === "NaN" ? '' : progress + '%');

    data = null;
};

var s4 = function() {
    return Math.floor((1 + Math.random()) * 0x10000)
        .toString(16)
        .substring(1);
};

var guid = function () {
    return s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
};

var formatPercentage = function (floatValue) {
    return (floatValue * 100).toFixed(2);
};

var formatFileSize = function (bytes) {
    if (typeof bytes !== 'number') {
        return '';
    }
    if (bytes >= 1000000000) {
        return (bytes / 1000000000).toFixed(2) + ' GB';
    }
    if (bytes >= 1000000) {
        return (bytes / 1000000).toFixed(2) + ' MB';
    }
    return (bytes / 1000).toFixed(2) + ' KB';
};

var formatBitrate = function (bits) {
    if (typeof bits !== 'number') {
        return '';
    }
    if (bits >= 1000000000) {
        return (bits / 1000000000).toFixed(2) + ' Gbit/s';
    }
    if (bits >= 1000000) {
        return (bits / 1000000).toFixed(2) + ' Mbit/s';
    }
    if (bits >= 1000) {
        return (bits / 1000).toFixed(2) + ' kbit/s';
    }
    return bits.toFixed(2) + ' bit/s';
};