window.blazorCulture = {
    get: () => {
        return localStorage.getItem('BlazorCulture');
    },
    set: (value) => {
        localStorage.setItem('BlazorCulture', value);
    }
};