import request, { type BasePageQuery } from "../modules/Request";
import { type ArticleSourceModel } from "./artcileSource";

export interface ArticleSaveModel {
  articleId?: string;
  title?: string;
  content?: string
  abstract?: string;
  articleTrueContent?:string;
  userId: number;
  isDraft: boolean;
  sources: ArticleSourceModel[];
}

export const ArticleStatus = {
  Published: 1,
  Auditing: 2,
  Audited: 3,
  NotPassed: 4
} as const;

export type ArticleStatus = (typeof ArticleStatus)[keyof typeof ArticleStatus];

export const ArticleStatusLabels: Record<ArticleStatus, string> = {
  [ArticleStatus.Published]: '已发布',
  [ArticleStatus.Auditing]: '审核中',
  [ArticleStatus.Audited]: '已审核',
  [ArticleStatus.NotPassed]: '未通过'
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
}

export interface UserArticleQuery extends BasePageQuery
{
   userId:number,
   isDraft:boolean,
   publishedStatus?:ArticleStatus,
   startDate?:string,
   endDate?:string,
   isDeleted?:boolean,
}

export function SaveArticle(model:ArticleSaveModel){
    return request.put("/api/Article/SaveArticle",model);
}

export function GetArticleContent(articleId:string){
    return request.get(`/api/Article/GetArticleContent/${articleId}`);
}

export function GetUserArticles(query:UserArticleQuery)
{
   return request.get("/api/Article/GetUserArticles",{params:query});
}

export function GetRecentArticles(query:BasePageQuery){
  return request.get("/api/Article/GetRecentArticles",{params:query});
}