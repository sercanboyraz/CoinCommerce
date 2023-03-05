function callWallet(symbol) {
    if (symbol != undefined && symbol != null)
        $.ajax({
            type: "GET",
            url: "/Plugins/PaymentCoin/WalletCoin?currencyCode=" + symbol,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: async function (msg) {
                let web3 = new Web3(Web3.givenProvider || "ws://localhost:8545");
                const wei = web3.utils.toWei('3', 'ether')
                let walletAddress = msg.WalletAddress;
                let chainId = msg.SelectChainId;
                let GasPrice = msg.GasPrice;
                let GasLimit = msg.GasLimit;
                let price = msg.OrderTotal;
                if (GasLimit != undefined) {
                    //let plain = 'Hi, you request a login from client to Eth Jwt Api. Please sign this message. This is not a transaction, is completely free and 100% secure. We\'ll use your signature to prove your ownership over your private key server side.';
                    //let msg = ethUtil.bufferToHex(new Buffer(plain, 'utf8'));
                    //let hash = ethUtil.bufferToHex(ethUtil.keccak256("\x19Ethereum Signed Message:\n" + plain.length + plain));
                    //let from = coinbase;

                    //let params = [msg, from];
                    let accounts = await ethereum.request({ method: 'eth_requestAccounts' });
                    let account = accounts[0];
                    console.log("account:" + account)

                    //await signed(account);
                    //let accountspersonal_sign = await ethereum.request({ method: method});
                    //console.log("accountspersonal_sign:" + accountspersonal_sign)

                    let valueLimit = decimalToHex(price);
                    let gasLimit = GasLimit.toString(16);
                    let gasPrice = GasPrice.toString(16);
                    const transactionParameters = {
                        gasPrice: gasPrice, // customizable by user during MetaMask confirmation.
                        gas: gasLimit, // customizable by user during MetaMask confirmation.
                        to: walletAddress, // Required except during contract publications.
                        from: accounts[0], // must match user's active address.
                        value: parseInt(web3.utils.toWei(price.toString(), "ether")).toString(16),//valueLimit,//"0x" + valueLimit, // Only required to send ether to the recipient from the initiating external account.
                        //data: '0x7f7465737432000000000000000000000000000000000000000000000000000000600057', // Optional, but used for defining smart contract creation and interaction.
                        //chainId: ethereum.networkVersion,
                        chainId: chainId
                    };
                    console.log("price.toString() :   " + price.toString())
                    console.log("valueLimit :   " + valueLimit);
                    ShowProgress();
                    await ethereum.request({ method: 'eth_sendTransaction', params: [transactionParameters] })
                        .then((txHash) => {
                            $.ajax({
                                type: "GET",
                                url: "/Plugins/PaymentCoin/Confirm?txt=" + txHash,
                                contentType: "application/json; charset=utf-8",
                                dataType: "json",
                                success: function (msg) {

                                }
                            });
                        })//order tablosunda tutulacak
                        .catch((error) => {
                            $.ajax({
                                type: "GET",
                                url: "/Plugins/PaymentCoin/Error?error=" + error,
                                contentType: "application/json; charset=utf-8",
                                dataType: "json",
                                success: function (msg) {

                                }
                            });
                            CloseProgress();
                        });// kullanıcıya dönülecek.
                }
            }
        });
};

async function signed(account) {
    const personalSignResult = document.querySelector('.showSigned');
    const exampleMessage = 'Example_personal_sign_message.';
    try {
        const from = account;
        const msg = "0x" + toHex(exampleMessage);
        const sign = await ethereum.request({
            method: 'personal_sign',
            params: [msg, from, 'Example password'],
        });
        personalSignResult.innerHTML = sign;
    } catch (err) {
        console.error(err);
        personalSign.innerHTML = `Error: ${err.message}`;
    }
}

function toHex(str) {
    var result = '';
    for (var i = 0; i < str.length; i++) {
        result += str.charCodeAt(i).toString(16);
    }
    return result;
}

function decimalToHex(d, padding) {
    var hex = Number(d).toString(16);
    padding = typeof (padding) === "undefined" || padding === null ? padding = 2 : padding;
    while (hex.length < padding) {
        hex = "0" + hex;
    }
    return hex;
}

function ShowProgress() {
    setTimeout(function () {
        var modal = $('<div />');
        modal.addClass("modalpayment");
        $('body').append(modal);
        var loading = $(".loading");
        loading.show();
        var top = Math.max($(window).height() / 2 - loading[0].offsetHeight / 2, 0);
        var left = Math.max($(window).width() / 2 - loading[0].offsetWidth / 2, 0);
        loading.css({ top: top, left: left });
    }, 200);
}

function CloseProgress() {
    var elems = document.getElementsByClassName("modalpayment");
    [].forEach.call(elems, function (el) {
        el.classList.remove("modalpayment");
    });
}