import  request  from "../modules/Request";

export interface ArticleSourceModel {
  sourceId: number;
  fileName?: string;
  articleId: string;
  url: string;
  isHyperLink: boolean;
}

export function UploadFile(file:File,articleId:string)
{
  const formData = new FormData();
  formData.append("file",file);
  formData.append("articleId",articleId);
  return request.post("/api/Article/handleUpload",formData,{
    headers:{
        "Content-Type" : "multipart/formdata"
    }
  });
}
