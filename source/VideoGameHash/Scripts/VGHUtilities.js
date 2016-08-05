$(function () {

    ProcessData = function (el) {
        $('div.LatestNewsItem a').on('mouseover', function () {
            // Display tooltip if headline is too long
            var textDim = $.textMetrics($(this));
            var divWidth = $(this).parent().width();
            if (textDim.width + 12 >= divWidth) { // add hardcode to account for the ellipse
                $(this).attr('rel', 'tooltip');
                $(this).tooltip({
                    title: $(this).text()
                });
                $(this).tooltip('show');
            }
        });
    }

    ProcessListData = function (el) {
        $('div.clipped-text a').on('mouseover', function () {

            // Display tooltip if headline is too long
            var textDim = $.textMetrics($(this));
            var divWidth = $('div.clipped-text').outerWidth();
            if (textDim.width + 3 >= divWidth) { // add hardcode to account for the ellipse
                $(this).attr('rel', 'tooltip');
                $(this).tooltip({
                    title: $(this).text()
                });
                $(this).tooltip('show');
            }
        });

        $('a.game-details').on('click', function () {
            var $url = $(this).data('url');
            var $detailswindow = document.getElementById('database-details');
            $($detailswindow).hide();

            var jqxhr = $.post($url)
                .done(function (data) {
                    $($detailswindow).html(data);
                    $($detailswindow).fadeIn('slow');
                    ProcessDetailsData(data);
                });
        });
    }

    ProcessDetailsData = function (el) {
        $('.thumbnailrightwrapper').hide();

        $('.article_body, .article_body_small').on('mouseenter', function () {

            var rightbarid = $(this).data('rightbarid');

            var $rightSideBar = document.getElementById(rightbarid);

            if ($rightSideBar != null) {
                $($rightSideBar).fadeIn('slow');
            }
        });

        $('.article_body, .article_body_small').on('mouseleave', function () {

            var rightbarid = $(this).data('rightbarid');

            var $rightSideBar = document.getElementById(rightbarid);

            if ($rightSideBar != null && $rightSideBar.childElementCount > 0) {
                $($rightSideBar).fadeOut('slow');
            }
        });

        var scrollBar = $('#scrollbar1');

        if (scrollBar.length > 0) {
            scrollBar.tinyscrollbar();

            var details = document.getElementById('detailsview');
            var height = details.scrollHeight;

            if (height <= 150) {
                $('.scrollbar').hide();
            }
        }

        $('div.LatestNewsItem a').on('mouseover', function () {
            // Display tooltip if headline is too long
            var textDim = $.textMetrics($(this));
            var divWidth = $('div.LatestNewsItem').outerWidth();
            if (textDim.width + 3 >= divWidth) { // add 6hardcode to account for the ellipse
                $(this).attr('rel', 'tooltip');
                $(this).tooltip({
                    title: $(this).text()
                });
                $(this).tooltip('show');
            }
        });

        $('a.game-details-systems').on('click', function () {
            var $url = $(this).data('url');
            var $detailswindow = document.getElementById('database-details');
            $($detailswindow).hide();

            var jqxhr = $.post($url)
                .done(function (data) {
                    $($detailswindow).html(data);
                    $($detailswindow).fadeIn('slow');
                    ProcessDetailsData(data);
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
                ProcessData(data);
                var child = $($this).find('span.glyphicon-chevron-right');
                if ($(child) != null) {
                    $(child).addClass('glyphicon-white');
                }
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

        $('#infometricstab').on('click', function () {
            // Pie Chart
            var infometricsPieChart = document.getElementById('infometricspiechart');
            if (infometricsPieChart.childNodes.length <= 0) {
                var infometricsPieChartUrl = $(infometricsPieChart).data('url');
                var jqxhr = $.post(infometricsPieChartUrl)
                     .done(function (data) {
                         $('#loadingpiechart').fadeOut('fast').hide();
                         $('#loadingpiechart').promise().done(function () {
                             $(infometricsPieChart).html(data);
                         });
                     });
            }


            // Line Chart
            var infometricsLineChart = document.getElementById('infometricslinechart');
            if (infometricsLineChart.childNodes.length <= 0) {
                var infometricsLineChartUrl = $(infometricsLineChart).data('url');
                var jqxhr = $.post(infometricsLineChartUrl)
                            .done(function (lineChartData) {
                                $('#loadinglinechart').fadeOut('fast').hide();
                                $('#loadinglinechart').promise().done(function () {
                                    $(infometricsLineChart).html(lineChartData);
                                });
                            });
            }


        });
    }

    ProcessInfometricsPieChart = function () {

        var what = this;
    }

    ProcessTrendingData = function (el) {
        var trendingScrollId = document.getElementById($(el).data('scrollid'));

        var oScrollbar5 = $(trendingScrollId);

        if (oScrollbar5.length > 0) {
            oScrollbar5.tinyscrollbar();

            var details = $(el).data('scrollviewid');
            var $details = document.getElementById(details);
            var height = $($details).actual('height');

            if (height >= 385) {
                var scrollgraphic = document.getElementById($(el).data('scrollgraphic'));
                $(scrollgraphic).show();
            }
        }


        $('div.LatestNewsItem a').on('mouseover', function () {
            // Display tooltip if headline is too long
            var textDim = $.textMetrics($(this));
            var divWidth = $(this).parent().width();
            if (textDim.width + 12 >= divWidth) { // add hardcode to account for the ellipse
                $(this).attr('rel', 'tooltip');
                $(this).tooltip({
                    title: $(this).text()
                });
                $(this).tooltip('show');
            }
        });
    }

    $.textMetrics = function (el) {

        var h = 0, w = 0;

        var div = document.createElement('div');
        document.body.appendChild(div);
        $(div).css({
            position: 'absolute',
            left: -1000,
            top: -1000,
            display: 'none'
        });

        $(div).html($(el).html());
        var styles = ['font-size', 'font-style', 'font-weight', 'font-family', 'line-height', 'text-transform', 'letter-spacing'];
        $(styles).each(function () {
            var s = this.toString();
            $(div).css(s, $(el).css(s));
        });

        h = $(div).outerHeight();
        w = $(div).outerWidth();

        $(div).remove();

        var ret = {
            height: h,
            width: w
        };

        return ret;
    };

    $.fn.hasOverflow = function () {
        var $this = $(this);
        var $children = $this.find('*');
        var len = $children.length;

        if (len) {
            var maxWidth = 0;
            var maxHeight = 0
            $children.map(function () {
                maxWidth = Math.max(maxWidth, $(this).outerWidth(true));
                maxHeight = Math.max(maxHeight, $(this).outerHeight(true));
            });

            return maxWidth > $this.width() || maxHeight > $this.height();
        }

        return false;
    };
});