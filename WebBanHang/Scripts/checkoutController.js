





var Checkout = {
            

    SetOrder: (fullname, phone, address, city, note) => {
        $.ajax({
            data: { fullname, phone, address, city, note},
            type: "get",
            url: `/Order/PaymentWithPaypal`,
            datatype: "json",
            success: function (res) {
                console.log(res)

                //$('#liveToast').toggleClass('show');
                //setTimeout(() => {
                //    $('#liveToast').removeClass('show');
                //}, 5000)
            },
            error: function (err) {
                console.log(err.responseText);
                alert("CO LOI XAY RA");
            }
        });
    }
}