export const toInputDate = (d) => (d ? new Date(d).toISOString().slice(0,10) : '')
export const fromInputDate = (s) => (s ? new Date(s) : null)