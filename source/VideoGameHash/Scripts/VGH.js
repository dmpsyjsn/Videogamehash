 /* File Created: March 28, 2013 */
$(function () {
    $('#loading').hide();

    $('.carousel-interval').carousel({
        interval: 8000 // in milliseconds
    });

    $('.trending_carousel').carousel({
        interval: 0 // in milliseconds
    });

    $('.dropdown-toggle').click(function () {
        $(this).next('.dropdown-menu').slideToggle(100);
    });

    $('#trendingTab a:first').tab('show');

    $('div.thumbnailrightwrapper, div.thumbnailrightwrappergrid').hide();

    $('#vgh_nav').affix({
        offset: $('#vgh_nav').position()
    });

    $('div#vgh_nav').hide();

    // Firefox fix - Due to FF's autocomplete feature,
    // the Poll's vote button may be disabled on page reload
    // check for that here
    if ($('.poll_button').is(':disabled') == true) {
        $('.poll_button').removeAttr('disabled');
    }


    var databaselist = $('#database-list');

    if (databaselist.length > 0) {
        var $url = $('#database-list').data('url');
        if ($url != "") {
            var $listwindow = document.getElementById('database-list');
            $($listwindow).hide();
            if ($url != null && $url != "") {

                var jqxhr = $.post($url)
            .done(function (data) {
                $($listwindow).html(data);
                $($listwindow).show('slow');
                ProcessListData(data);
            });
            }
        }
    }

    var databasegamedetails = $('#database-details');

    if (databasegamedetails.length > 0) {
        var $url = $('#database-details').data('url');
        if ($url != "") {
            var $detailswindow = document.getElementById('database-details');
            $($detailswindow).hide();

            var jqxhr = $.post($url)
                .done(function (data) {
                    $($detailswindow).html(data);
                    $($detailswindow).fadeIn('slow');
                    ProcessDetailsData(data);
                });
        }
    }

    $('.article_body, .article_body_small, .thumbnail, .thumbnail_small').on('mouseenter', function () {

        var rightbarid = $(this).data('rightbarid');

        var $rightSideBar = document.getElementById(rightbarid);

        if ($rightSideBar != null) {
            $($rightSideBar).fadeIn('slow');
        }
    });

    $('.article_body, .article_body_small,  .thumbnail, .thumbnail_small').on('mouseleave', function () {

        var rightbarid = $(this).data('rightbarid');

        var $rightSideBar = document.getElementById(rightbarid);

        if ($rightSideBar != null && $rightSideBar.childElementCount > 0) {
            $($rightSideBar).fadeOut('slow');
        }
    });

    $('.trending-stories-first').each(function () {
        var $this = $(this);
        var $url = $(this).data('url');
        var jqxhr = $.post($url)
            .done(function (data) {
                $this.html(data);
                $($this).fadeIn('slow').delay(250);
                $($this).promise().done(function () {
                    ProcessTrendingData($this);
                });
            });
    });

    $('.Trending_Tab').on('click', function () {

        if (!$(this).hasClass('active')) {
            var $this = $(this);
            var trendingSection = $($this).data('trendingsectionid');
            var $trendingSection = document.getElementById(trendingSection);
            $($trendingSection).empty();
            var $url = $($this).data('url');
            var jqxhr = $.post($url)
                        .done(function (data) {
                            $($trendingSection).html(data);
                            $($trendingSection).fadeIn('slow').delay(500);
                            $($trendingSection).promise().done(function () {
                                ProcessTrendingData($trendingSection);
                            });
                        });
        }
    });

    $('.LatestInfoSection').each(function () {

        var $this = $(this);
        var $url = $(this).data('url');
        var loadingAttr = $(this).data('loadingid');
        var $loadingWindow = document.getElementById(loadingAttr);

        $($loadingWindow).show();

        $.ajax({
            type: 'POST',
            url: $url,
            data: "",
            success: function (viewHTML) {
                $($loadingWindow).hide();
                $this.html(viewHTML);
                $this.fadeIn('slow');
                ProcessData($this);
            }
        });
    });

    $('.thumbnailrightwrapper').on('click', function () {
        $(this).attr('disabled', 'disabled');
        var $this = $(this);
        var secId = $(this).data('mainid');
        var $secondWindow = document.getElementById(secId);
        var loadingId = $(this).data('loadingid');
        var $loadingWindow = document.getElementById(loadingId);

        if ($secondWindow.childElementCount <= 1) {
            $($loadingWindow).show();
            var $url = $(this).data('url');
            var jqxhr = $.post($url)
            .done(function (data) {
                $($loadingWindow).hide();
                $($secondWindow).html(data);
                $($secondWindow).fadeIn('slow');

                var child = $($this).find('span.glyphicon-chevron-right');
                if ($(child) != null) {
                    $(child).addClass('glyphicon-white');
                }

                ProcessData($secondWindow);
            });
        }
        else {
            var child = $($this).find('span.glyphicon-chevron-right');
            if ($(child) != null) {
                if ($(child).hasClass('glyphicon-white')) {
                    $(child).removeClass('glyphicon-white');
                    $($secondWindow).hide('slow');
                }
                else {
                    $(child).addClass('glyphicon-white');
                    $($secondWindow).show('slow');
                }
            }
        }

        $(this).removeAttr('disabled');
    });

    $('.thumbnailrightwrappergrid').on('click', function () {
        $(this).attr('disabled', 'disabled');
        var $this = $(this);
        var contentId = $(this).data('contentid');
        var $content = document.getElementById(contentId);

        var secId = $(this).data('mainid');
        var $secondWindow = document.getElementById(secId);

        var loadingId = $(this).data('loadingid');
        var $loadingWindow = document.getElementById(loadingId);

        var wrapperClassId = $(this).data('classwrapper');
        var $wrapperclass = document.getElementById(wrapperClassId);

        $($content).hide();

        if ($secondWindow.childElementCount <= 1) {
            $($loadingWindow).show();
            var $url = $(this).data('url');
            var jqxhr = $.post($url)
            .done(function (data) {
                $($loadingWindow).hide();
                $($secondWindow).html(data);
                $($secondWindow).fadeIn('slow');

                var child = $($this).find('span.glyphicon-chevron-left');
                if ($(child) != null) {
                    $(child).addClass('glyphicon-white');
                }

                if ($wrapperclass != null) {
                    $($wrapperclass).addClass('no-before');
                    $($wrapperclass).removeClass('gradient');
                }

                ProcessData($secondWindow);
            });
        }
        else {
            var child = $($this).find('span.glyphicon-chevron-left');
            if ($(child) != null) {
                if ($(child).hasClass('glyphicon-white')) {
                    $(child).removeClass('glyphicon-white');
                    $($secondWindow).hide('slide', { direction: 'right' }, 350);
                    $($secondWindow).promise().done(function () {
                        $($content).fadeIn('slow');
                    });

                    if ($wrapperclass != null) {
                        $($wrapperclass).removeClass('no-before');
                        $($wrapperclass).addClass('gradient');
                    }
                }
                else {
                    $(child).addClass('glyphicon-white');
                    $($content).hide();

                    if ($wrapperclass != null) {
                        $($wrapperclass).addClass('no-before');
                        $($wrapperclass).removeClass('gradient');
                    }

                    $($secondWindow).show('slide', { direction: 'right' }, 350);
                }
            }
        }

        $(this).removeAttr('disabled');
    });

    $('.toolbox').on('load', function () {
        if ($(this).hasOverflow()) {
            $(this).addClass('toolbox_hide');
        }

    });

    $('a.database-button').on('click', function () {
        $('a.database-button').each(function () {
            if ($(this).hasClass('active')) {
                $(this).removeClass('active');
            }
        });
        $(this).addClass('active');
        var $url = $(this).data('url');
        var $listwindow = document.getElementById('database-list');
        $($listwindow).hide();

        var jqxhr = $.post($url)
            .done(function (data) {
                $($listwindow).html(data);
                $($listwindow).show('slow');
                ProcessListData(data);
            });

    });

    $('#searchdatabaseform').submit(function () {
        var queryString = $('#searchdatabaseform').formSerialize();

        var $url = $(this).data('url');
        var $listwindow = document.getElementById('database-list');
        $($listwindow).hide();

        var jqxhr = $.post($url, queryString)
            .done(function (data) {
                $($listwindow).html(data);
                $($listwindow).show('slow');
                ProcessListData(data);
            });

        return false;
    });

    $('header#vgh_master_header').on('mouseover', function () {
        var nav_bar = document.getElementById('vgh_scroll_nav');
        if (isElementInViewport(nav_bar) == false) {
            $('#vgh_nav').fadeIn('fast');
        }
    });

    $('header#vgh_master_header').on('mouseleave', function () {
        if ($('#vgh_nav').is(':visible')) {
            $('#vgh_nav').fadeOut('fast');
        }
    });

    $('.nav-pills > li.trending-pills').on('click', function (e) {
        e.preventDefault();
        if (!$(this).hasClass('active')) {
            var $this = $(this);
            // Remove active class from previous
            $(this).siblings().each(function () {
                if ($(this).hasClass('active')) {
                    $(this).removeClass('active');
                }
            });
            $(this).addClass('active');

            var carouselIndexVal = $(this).data('carouselindex');
            var carouselId = $(this).data('carouselid');
            var $carouselElm = document.getElementById(carouselId);
            $($carouselElm).carousel(carouselIndexVal);

            $($carouselElm).promise().done(function () {
                var trendingSection = $($this).data('trendingsectionid');
                var $trendingSection = document.getElementById(trendingSection);
                var $url = $($this).data('url');
                $($trendingSection).fadeOut('slow');
                $($trendingSection).promise().done(function () {
                    var jqxhr = $.post($url)
                        .done(function (data) {
                            $($trendingSection).html(data);
                            $($trendingSection).fadeIn('slow').delay(250);
                            $($trendingSection).promise().done(function () {
                                ProcessTrendingData($trendingSection);
                            });
                        });
                });
            });
        }
    });

    $('.poll_button').on('click', function () {
        // Disable button
        $(this).attr('disabled', 'disabled');

        var url = $(this).data('url');

        var field = $(this).data('fieldid');
        var $field = document.getElementById(field);

        var radioDiv = $(this).data('radiodivid');
        var $radioDiv = document.getElementById(radioDiv);

        var chartId = $(this).data('chartid');
        var $chartId = document.getElementById(chartId);

        var votedID = null;

        var subId = chartId + "_sub";

        $($field).find('input').each(function () {
            if ($(this).is(':checked')) {
                votedID = $(this).val();
            }
        });

        if (votedID != null) {
            url += "&PollVal=" + votedID;
            var jqxhr = $.getJSON(url, function (data) {
                $($radioDiv).fadeOut('slow');
                $($radioDiv).promise().done(function () {
                    var total_votes = 0;
                    var percent;
                    for (id in data) {
                        var value = data[id];
                        if (!isNaN(value['NumVotes'])) {
                            total_votes += parseInt(value['NumVotes']);
                        }
                    }
                    var results_html = "<div class='poll-results' id='" + subId + "'><dl class='graph'>\n";
                    for (id in data) {
                        var value = data[id];
                        if (!isNaN(value['NumVotes'])) {
                            var pollId = value['Id'];
                            percent = Math.round((parseInt(value['NumVotes']) / parseInt(total_votes)) * 100);

                            if (pollId != votedID) {
                                results_html += "<dt class='bar-title'>" + value['Title'] + "</dt><dd class='bar-container'><div id='bar" + value['Id'] + "'style='width:0%;'>&nbsp;</div><strong>" + percent + "%</strong></dd>\n";
                            }
                            else {
                                results_html += "<dt class='bar-title'><strong>" + value['Title'] + "</strong></dt><dd class='bar-container'><div id='bar" + value['Id'] + "'style='width:0%;background-color:#0066cc;'>&nbsp;</div><strong>" + percent + "%</strong></dd>\n";
                            }
                        }
                    }
                    results_html = results_html + "</dl></div><div style=\"width:200px; text-align=center; margin-left:50px\"><span>Total Votes: " + total_votes + "</span></div>\n";
                    $($chartId).append(results_html).fadeIn("slow", function () {
                        $('#' + subId + ' div').each(function () {
                            var percentage = $(this).next().text();
                            $(this).css({ width: "0%" }).animate({
                                width: percentage
                            }, 'slow');
                        });
                    });
                });
            });
        }
    });

    $('.info_options a').on('click', function () {
        $('#info_actions').hide();
        $('#loading').fadeIn('slow');
    });

    function isElementInViewport(el) {
        var rect = el.getBoundingClientRect();

        return (
        rect.top >= 0 &&
        rect.left >= 0 &&
        rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) && /*or $(window).height() */
        rect.right <= (window.innerWidth || document.documentElement.clientWidth) /*or $(window).width() */
        );
    }

    $("#importDialog").dialog({
        autoOpen: false,
        height: 300,
        width: 600,
        modal: true,
        buttons: {
            "Ok": function () {
                $(this).dialog("close");
            }
        }
    });

    $("#jsonImportLink").click(function () {
        $("#importDialog").dialog("open");
    });
});
