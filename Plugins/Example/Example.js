//this is example js script, use it to grab the product's attributes
//accepted format:
//'[
// {"key":"attribute name","value":"attribute value"},
// .....
// {"key":"another attribute name","value":"attribute value"}
// ]'

//following example would work on element with class example containing a table:
//<table>
//.......
//<tr>
//<td>key</td><td>value</td>
//</tr>
//......
//</table>
window.runScript = () => {
    const result = [];
    const items = document.querySelectorAll('.example tr')

    items.forEach(item => {
        const keyNode = item.childNodes[1];
        const valNode = item.childNodes[3];
        const key = keyNode?.textContent?.trim() ?? "";
        const value = valNode?.textContent?.trim() ?? "";
        result.push({ key, value });
    });
    return JSON.stringify(result);
};