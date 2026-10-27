import axios from 'axios'
import stateStroge from './StateStorage'
import type { User } from '../api/user'
import type { AxiosRequestConfig } from 'axios'

interface ResponseResult{
  success: boolean;
  code: number;
  message: string;
  data: any;
}
export const baseUrl = 'http://localhost:5575';
export function getSource(sourceName:string,sourceKey?:string){
  let src = baseUrl;
  if(sourceKey)
    src += "/files/" + sourceKey;
  return `${src}/${sourceName}`;
}
const request = axios.create({
  baseURL: baseUrl, // Replace with your API base URL
  timeout: 1000*60*3,
});
request.interceptors.request.use(
  config => {
    const user = stateStroge.get<User>("user")
    if (user) 
      config.headers.Authorization = `Bearer ${user.token}`
  
    return config
  },
  error => {
    return Promise.reject(error)
  }
)

export class HttpRequest {
  get(url: string, config?: AxiosRequestConfig) {
    return request.get(url, config).then(response => response.data as ResponseResult);
  }

  post(url: string, data?: any, config?: AxiosRequestConfig) {
    return request.post(url, data, config).then(response => response.data as ResponseResult);
  }

  put(url: string, data?: any, config?: AxiosRequestConfig) {
    return request.put(url, data, config).then(response => response.data as ResponseResult);
  }

  delete(url: string, config?: AxiosRequestConfig) {
    return request.delete(url, config).then(response => response.data as ResponseResult);
  }

  patch(url: string, data?: any, config?: AxiosRequestConfig) {
    return request.patch(url, data, config).then(response => response.data as ResponseResult);
  }
}
const instance = new HttpRequest();
export default instance;