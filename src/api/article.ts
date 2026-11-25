import request, { type BasePageQuery } from "../modules/Request";

export interface ArticleSaveModel {
  articleId?: string;
  title?: string;
  content?: string
  abstract?: string;
  // 文章封面地址，为空时使用默认封面
  cover?: string;
  // 创作声明编号，对应 CreationDecalrations 中的 value
  creationDeclaration?: string;
  articleTrueContent?:string;
  // 上次保存时的原文，供服务端比对 HTML 内嵌资源的增删
  articleOriginalContent?:string;
  userId: number;
  isDraft: boolean;
}

export const ArticleStatus = {
  Published: 1,
  Auditing: 2,
  Audited: 3,
  NotPassed: 4,
  Editing:5
} as const;

export type ArticleStatus = (typeof ArticleStatus)[keyof typeof ArticleStatus];

export const ArticleStatusLabels: Record<ArticleStatus, string> = {
  [ArticleStatus.Published]: '已发布',
  [ArticleStatus.Auditing]: '审核中',
  [ArticleStatus.Audited]: '已审核',
  [ArticleStatus.NotPassed]: '未通过',
  [ArticleStatus.Editing] : "编辑中"
}

export interface ArticleListItem {
  articleId: string;
  title: string;
  isDraft:boolean,
  abstract?: string;
  authorId: number;
  authorName: string;
  authorAvatar: string;
  createTime: string;
  uploadTime?:Date,
  publishedStatus: ArticleStatus;
  viewCount: number;
  likeCount: number;
  collectCount: number;
  saveWhenInit:boolean;
  creationDeclaration?:string;
  cover?:string;
}

export interface UserArticleQuery extends BasePageQuery
{
   userId:number,
   isDraft?:boolean,
   publishStatus?:ArticleStatus,
   startDate?:string,
   endDate?:string,
   isDeleted?:boolean,
}

export interface ArticleDetail {
  articleId: string;
  title: string;
  abstract?: string;
  cover?: string;
  content?: string;
  creationDeclaration?: string;
  authorId: number;
  authorName: string;
  authorAvatar: string;
  viewCount: number;
  likeCount: number;
  dislikeCount: number;
  collectCount: number;
  commentCount: number;
  createTime?: string;
  publishTime?: string;
  liked: boolean;
  disliked: boolean;
  collected: boolean;
}

export const LikeOrDislike = {
  Like: 'Like',
  Dislike: 'Dislike'
} as const;

export type LikeOrDislike = (typeof LikeOrDislike)[keyof typeof LikeOrDislike];

export function SaveArticle(model:ArticleSaveModel){
    return request.put("/api/Article/SaveArticle",model);
}

export function GetArticleContent(articleId:string){
    return request.get(`/api/Article/GetArticleContent/${articleId}`);
}

export function GetArticleDetail(articleId:string){
    return request.get(`/api/Article/detail/${articleId}`);
}

export function ViewArticle(articleId:string){
    return request.patch(`/api/Article/ViewArticle/${articleId}`);
}

export function LikeOrDislikeArticle(articleId:string,type:LikeOrDislike){
    return request.put(`/api/Article/LikeOrDislikeArticle/${articleId}`,undefined,{
        params:{type}
    });
}

export function CollectArticle(articleId:string,toCollect:boolean){
    return request.put(`/api/Article/CollectArticle/${articleId}`,undefined,{
        params:{toCollect}
    });
}

export function GetUserArticles(query:UserArticleQuery)
{
   return request.get("/api/Article/GetUserArticles",{params:query});
}

export function GetRecentArticles(query:BasePageQuery){
  return request.get("/api/Article/GetRecentArticles",{params:query});
}

export function RemoveArticle(articleId:string,trueDelete?:boolean)
{
  const doTrueDelete = trueDelete!=undefined && trueDelete;
  return request.delete(`/api/Article/RemoveArticle/${articleId}`,{
    params:{trueDelete:doTrueDelete}
  });
}

export function PublishArticle(articleId:string)
{
  return request.post(`/api/Article/PublishArticle/${articleId}`);
}