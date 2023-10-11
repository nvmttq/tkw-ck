




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
    }
}