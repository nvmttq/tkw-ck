
var adminProduct = {

    AuthorizeUser: () => {
        var returnUrl = window.location.href;
        $.ajax({
            data: { returnUrl},
            type: "get",
            url: `/Admin/Dashboard/AuthorizeLogin`,
            datatype: "json",
            success: function (res) {

                if (res.code === 103) return;
                if (res.code === 200) {
                    window.location.href = returnUrl;

                } else {
                    window.location.href = "https://localhost:44302/Admin/Dashboard/LoginAdmin";
                }

            },
            error: function (err) {
                console.log(err.responseText);
            }
        });
    },

    EditPro: (product) => {

        var chips = "";
        document.querySelectorAll(".chips .chip").forEach(n => {
            if (chips === "") chips = n.querySelector("span").textContent;
            else chips += "," + n.querySelector("span").textContent;
        });

        console.log(chips, product)

        $.ajax({
            data: {chips,product},
            type: "post",
            url: `/Admin/ProductManagerment/Edit`,
            datatype: "json",
            success: function (res) {

                console.log(res)

            },
            error: function (err) {
                console.log(err.responseText);
            }
        });
    },



}