import { sortBy } from 'underscore';

export class CommonUtils {
    static sortArray(array: any[], key: string, isAscending = true) {
        const sorted = sortBy(array, key);
        return isAscending ? sorted : sorted.reverse();
    }
}
