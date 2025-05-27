import axios from 'axios';

const apiClient = axios.create({
    baseURL: 'https://cpms-api.jollysky-76e311d7.northeurope.azurecontainerapps.io/api',
    headers: {
        'Content-Type': 'application/json',
    },
});

apiClient.interceptors.response.use(
    (response) => response,
    (error) => {
        console.error('API Error:', error);
        return Promise.reject(error);
    }
);

export default apiClient;