

const HandleCategoryId = function (categoryId, categoryIdParams) {
    var dataCategoryId = {
        url: (categoryIdParams ? categoryIdParams : ""),
        data: (categoryIdParams ? categoryIdParams : ""),
    };
    if (categoryId) {

        if (categoryIdParams) {
            var arrCategoryId = categoryIdParams.split(',');
            if (arrCategoryId.includes(categoryId.toString()) === false) arrCategoryId.push(categoryId.toString());
            else {

                var index = arrCategoryId.indexOf(categoryId.toString());
                console.log(index);
                if (index > -1) {
                    arrCategoryId = arrCategoryId.filter(item => item != categoryId.toString());
                }
                console.log("REMOVE", arrCategoryId)
            }
            console.log(arrCategoryId)
            dataCategoryId.url = arrCategoryId.join('%2C')
            dataCategoryId.data = arrCategoryId.join(",");
            console.log(dataCategoryId)

            if (arrCategoryId.length === 0) {
                dataCategoryId.data = "";
                dataCategoryId.url = ""
            }
        } else {
            dataCategoryId.data = categoryId.toString();
            dataCategoryId.url = `${categoryId.toString()}`;
        }

    }
    return dataCategoryId;
}


const HandlePaging = function (page, pageParams) {

    var urlPageParams = (pageParams ? pageParams : "");
    if (page) {
        urlPageParams = page.toString();
    }
    if (page === '1') urlPageParams = '1';
    return urlPageParams;
}

const HandleKeyword = function (keyword, keywordParams) {

    var urlKeywordParams = (keywordParams ? keywordParams : "");
    if (keyword) {
        urlKeywordParams = keyword.toString();
    }
 
    return urlKeywordParams;
}

var App = {

    ClearFilter: function (name) {
        var queryString = window.location.search;
        const urlParams = new URLSearchParams(queryString);
        urlParams.delete(name);
        if (name == "sortby") urlParams.delete("order");

        var categoryIdParams = urlParams.get('CategoryId')
        var keywordParams = urlParams.get('keyword');
        var pageParams = urlParams.get('page');
        var sortbyParams = urlParams.get('sortby')
        var orderParams = urlParams.get('order')


;        $.ajax({
            data: { CategoryId: categoryIdParams, pageCurrent: pageParams, keyword: keywordParams, sortby: sortbyParams, order: orderParams },
            type: "get",
            url: `/Home/Search?${urlParams.toString()}`,
            datatype: "json",
            success: function (res) {



                window.history.pushState("", "", `/Home/Search?${urlParams.toString()}`);

                $("#content-panel").empty();
                $("#content-panel").append(res);

            },
            error: function (err) {
                console.log(err.responseText);
            }
        });
    },

    SearchCategory: function (categoryId, keyword, page, sortby, order) {
        console.log(keyword)
        var queryString = window.location.search;
        const urlParams = new URLSearchParams(queryString);
        var categoryIdParams = urlParams.get('CategoryId')
        var keywordParams = urlParams.get('keyword');
        var pageParams = urlParams.get('page');
        var sortbyParams = urlParams.get('sortby')
        var orderParams = urlParams.get('order')


        var dataCategoryId = HandleCategoryId(categoryId, categoryIdParams);
        console.log(dataCategoryId)
        if (dataCategoryId.url !== "") {
            urlParams.set("CategoryId", `${dataCategoryId.data}`);
        } else {
            urlParams.delete("CategoryId");
        }

        var urlPageParams = HandlePaging(page, pageParams);
        if (urlPageParams !== "") {
            urlParams.set("page", urlPageParams);
        } else {
            urlParams.delete("page");
        }

        var urlKeywordParams = HandleKeyword(keyword, keywordParams);
        if (urlKeywordParams !== "") {
            urlParams.set("keyword", urlKeywordParams);
        } else {
            urlParams.delete("keyword");
        }

        if (sortbyParams) {
            if (sortby) {
                urlParams.set("sortby", sortby);
                urlParams.set("order", order)
            } else {
                urlParams.set("sortby", sortbyParams);
                urlParams.set("order", orderParams)
                sortby = sortbyParams;
                order = orderParams;
            }
        } else {
            if (sortby) {
                urlParams.set("sortby", sortby);
                urlParams.set("order", order)
            } else {
                urlParams.delete("sortby");
                urlParams.delete("order");
            }
            
        }

        console.log(`Home/Search/?${urlParams.toString()}`)

        $.ajax({
            data: { CategoryId: dataCategoryId.data, pageCurrent: urlPageParams, keyword: urlKeywordParams, sortby: sortby, order:order },
            type: "get",
            url: `/Home/Search?${urlParams.toString()}`,
            datatype: "json",
            success: function (res) {



                window.history.pushState("", "", `/Home/Search?${urlParams.toString()}`);

                $("#content-panel").empty();
                $("#content-panel").append(res);

            },
            error: function (err) {
                console.log(err.responseText);
            }
        });
    },

}