
export interface ValueLabel<T>{
    value: T;
    label: string;
}

export const CreationDecalrations : ValueLabel<number>[] =[
    {value: 0, label: '无声明'},
    {value: 1, label: '个人观点仅供参考'},
    {value: 2, label: '仅供学习交流使用'},
    {value: 3, label: '部分内容有AI辅助生成'},
    {value: 4, label: '内容来自网络，进行整合/再创作'}
]

export function GetLabelFFromValue<T>(value: T, arr: ValueLabel<T>[]):string | undefined {
    const item = arr.find(i => i.value === value);
    return item?.label;
}