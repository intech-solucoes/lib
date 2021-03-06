import axios from "axios";

var config = require("../../../src/config.json");

const apiUrl = config.apiUrl;

export default class BaseService {
    GetToken() {
        return new Promise((resolve, reject) => {
            try {
                if(typeof(localStorage) !== 'undefined') {
                    var token = localStorage.getItem("token");
                    resolve(token);
                } else {
                    var ReactNative = require("react-native");
                    ReactNative.AsyncStorage.getItem("token", (err, result) => err ? reject(err) : resolve(result));
                }
            }
            catch(e) {
                reject(e);
            }
        });
    }

    VerificarAdmin () {
        return new Promise((resolve, reject) => {
            axios({
                method: "GET",
                url: apiUrl + "/usuario/admin",
                data: {},
                headers: {
                    "Authorization": "Bearer " + localStorage.getItem("token-admin")
                }
            })
            .then(resolve)
            .catch(reject);
        });
    }

    CriarRequisicao(tipo, url, data = null) {
        return new Promise((resolve, reject) => {
            this.GetToken()
                .then(token => {
                    axios({
                        method: tipo,
                        url: apiUrl + url,
                        data: data,
                        headers: {
                            "Authorization": "Bearer " + token
                        }
                    })
                    .then(resolve)
                    .catch(reject);
                });
        });
    }

    CriarRequisicaoBlob(tipo, url, data = null) {
        return new Promise((resolve, reject) => {
            this.GetToken()
                .then(token => {
                    axios({
                        method: tipo,
                        url: apiUrl + url,
                        data: data,
                        headers: {
                            "Authorization": "Bearer " + token
                        },
                        responseType: 'blob'
                    })
                    .then(resolve)
                    .catch(reject);
                });
        });
    }

    CriarRequisicaoZip(tipo, url, data = null) {
        return new Promise((resolve, reject) => {
            this.GetToken()
                .then(token => {
                    axios({
                        method: tipo,
                        url: apiUrl + url,
                        data: data,
                        headers: {
                            "Authorization": "Bearer " + token
                        },
                        responseType: 'arraybuffer'
                    })
                    .then(resolve)
                    .catch(reject);
                });
        });
    }

    FormatarData(data) {
        return data.replace(new RegExp('/', 'g'), '.');
    }

    MontarRota(controller, versao, rota) {
        var rotaFinal = `/${controller}`;

        if(versao && versao !== "")
            rotaFinal = `${rotaFinal}/${versao}`;

        if(rota && rota !== "")
            rotaFinal = `${rotaFinal}/${rota}`;

        return rotaFinal;
    }
}