import axios from "axios";

const apiUrl = process.env.API_URL;

export default class BaseService {
    GetToken() {
        return new Promise((resolve, reject) => {
            try {
                if(localStorage) {
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

    FormatarData(data) {
        return data.replace(new RegExp('/', 'g'), '.');
    }
}
