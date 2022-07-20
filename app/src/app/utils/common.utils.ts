export class CommonUtils {
    static sortArray(array: any[], key: string, isAscending: boolean = true) {
        return array.sort((a, b) => {
            if (a[key] < b[key]) {
                return isAscending ? -1 : 1;
            } else if (a[key] > b[key]) {
                return isAscending ? 1 : -1;
            } else {
                return 0;
            }
        });
    }
}
