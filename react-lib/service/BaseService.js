import axios from "axios";

export default class BaseService {
    constructor(config) {
        this.config = config;
    }

    GetToken() {
        return new Promise((resolve, reject) => {
            try {
                try {
                    var ReactNative = require("react-native");
                    ReactNative.AsyncStorage.getItem("token", (err, result) => err ? reject(err) : resolve(result));
                } catch(e) {
                    var token = localStorage.getItem("token");
                    resolve(token);
                }
            } catch(e) {
                reject(e);
            }
        });
    }

    CriarRequisicao(tipo, url, data = null) {
        return new Promise((resolve, reject) => {
            this.GetToken()
                .then(token => {

                    axios({
                        method: tipo,
                        url: this.config.apiUrl + url,
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
                        url: this.config.apiUrl + url,
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

    FormatarData(data) {
        return data.replace(new RegExp('/', 'g'), '.');
    }
}
