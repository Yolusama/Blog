
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

export function GetLabelFromValue<T>(value: T, arr: ValueLabel<T>[]):string | undefined {
    const item = arr.find(i => i.value === value);
    return item?.label;
}

export function GetAgeWithMonth(date:Date)
{
    const now = new Date();
    if (Number.isNaN(date.getTime())) {
        throw new RangeError('date must be a valid Date');
    }

    const dayNumber = (year: number, month: number, day: number) => {
        const value = new Date(0);
        value.setUTCFullYear(year, month, day);
        return value.getTime() / 86_400_000;
    };
    const addMonths = (year: number, month: number, day: number, months: number) => {
        const targetMonth = month + months;
        const targetYear = year + Math.floor(targetMonth / 12);
        const normalizedMonth = ((targetMonth % 12) + 12) % 12;
        const lastDay = new Date(0);
        lastDay.setUTCFullYear(targetYear, normalizedMonth + 1, 0);
        return dayNumber(targetYear, normalizedMonth, Math.min(day, lastDay.getUTCDate()));
    };

    const start = [date.getFullYear(), date.getMonth(), date.getDate()];
    const end = [now.getFullYear(), now.getMonth(), now.getDate()];
    const startDay = dayNumber(start[0], start[1], start[2]);
    const endDay = dayNumber(end[0], end[1], end[2]);
    const isFuture = startDay > endDay;
    const [earlier, later] = isFuture ? [end, start] : [start, end];
    const laterDay = isFuture ? startDay : endDay;

    let totalMonths = (later[0] - earlier[0]) * 12 + later[1] - earlier[1];
    let monthAnchor = addMonths(earlier[0], earlier[1], earlier[2], totalMonths);
    if (monthAnchor > laterDay) {
        totalMonths--;
        monthAnchor = addMonths(earlier[0], earlier[1], earlier[2], totalMonths);
    }

    const years = Math.floor(totalMonths / 12);
    const months = totalMonths % 12;
    const days = laterDay - monthAnchor;
    const sign = isFuture ? '-' : '';
    return `${sign}${years}年${months}月${days}天`;
}