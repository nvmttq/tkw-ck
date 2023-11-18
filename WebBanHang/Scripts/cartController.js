




const Cart = {


    RemoveProduct: (productId) => {
        $.ajax({
            data: { productId: productId},
            type: "get",
            url: `/Cart/RemoveProduct?productId=${productId}`,
            datatype: "json",
            success: function (res) {
                window.location.reload();
            },
            error: function (err) {
                console.log(err.responseText);
            }
        });
    },

    SelectProductCheckout: (t, p) => {
        console.log(t, p);
        var type = (t.checked ? "ADD" : "REMOVE");
        var quantities = document.getElementById('quantities-product-cart').value;
        console.log(type, quantities)
       
        $.ajax({
            data: { p: p.product, c: p.cart, type: type},
            type: "post",
            url: `/Cart/SelectProductCheckout`,
            datatype: "json",
            success: function (res) {

                console.log(res);
            },
            error: function (err) {
                console.log(err.responseText);
            }
        });
    },

    UpdateQuantity: (productId, quantities) => {
        console.log(productId);
        $.ajax({
            data: { productId: productId, quantities: parseInt(quantities) },
            type: "get",
            url: `/Cart/UpdateQuantity?productId=${productId}`,
            datatype: "json",
            success: function (res) {

                console.log(`/Cart/UpdateQuantity?productId=${productId}`)
            },
            error: function (err) {
                console.log(err.responseText);
            }
        });
    },

    AddToCart: (productId, quantities) => {
        $.ajax({
            data: { productId: productId, quantities: parseInt(quantities) },
            type: "post",
            url: `/Cart/AddToCart`,
            datatype: "json",
            success: function (res) {

                if (res.code == 200) {
                    console.log(res);
                    $("#number-of-productcart").text(res.quantities.toString());
                } else {
                    window.location.href = "https://localhost:44302/Account/Login";
                }

            },
            error: function (err) {
                console.log(err.responseText);
            }
        });
    }
}