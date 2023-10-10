

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
var App = {


    Searching: function (keyword) {
        var queryString = window.location.search;
        const urlParams = new URLSearchParams(queryString);
        var CategoryIdParams = urlParams.get('CategoryId')

        var arrCategoryId = CategoryIdParams.split(',');

        var url = `/Home/Search?CategoryId=${CategoryIdParams}`;

        if (!keyword) {
            return;
        }

        url = `/Home/Search?CategoryId=${CategoryIdParams}&keyword=${keyword}`
        $.ajax({
            data: { categoryId: CategoryIdParams, keyword: keyword },
            type: "get",
            url: url,
            datatype: "json",
            success: function (res) {


                window.history.pushState("", "", `/Home/Search?CategoryId=${CategoryIdParams}&keyword=${keyword}`);

                $("#content-panel").empty();
                $("#content-panel").append(res);

            },
            error: function (err) {
                console.log(err.responseText);
            }
        });
    },

    SearchCategory: function (categoryId, keyword, page) {
        console.log(keyword)
        var queryString = window.location.search;
        const urlParams = new URLSearchParams(queryString);
        var categoryIdParams = urlParams.get('CategoryId')
        var keywordParams = urlParams.get('keyword');
        var pageParams = urlParams.get('page');
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
        console.log(`Home/Search/?${urlParams.toString()}`)

        $.ajax({
            data: { CategoryId: dataCategoryId.data, pageCurrent: urlPageParams },
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