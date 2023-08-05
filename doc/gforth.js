/* filter-search for gforth word index */
/* (c)copyright 2023 by Gerald Wodni  */

function assimilateSearch() {
    const table = document.querySelector("table.index-fn");

    /* insert search box */
    table.insertAdjacentHTML('afterbegin', `
        <tr>
            <td colspan="2">
                <input type="text" id="searchbox" placeholder="filter" autofocus="autofocus"/>
            </td>
        </tr>
    `);
    const searchBox = document.getElementById("searchbox");
    searchBox.focus();

    /* words */
    const sectionTrs = [...table.querySelectorAll("th:first-child")].map( th => th.closest("tr") );
    const emptyTrs = [...table.querySelectorAll("td[colspan='4']")].map( td => td.closest("tr") );
    const wordTrs = [...table.querySelectorAll("code")].map( code => { return {
        word: code.textContent.split(" ")[0],
        tr: code.closest("tr"),
    } });

    /* filter */
    function trVisible( tr, visible ) {
        tr.style.display = visible ? "table-row" : "none";
    }
    searchBox.addEventListener("keyup", evt => {
        const needle = searchBox.value.trim();
        console.log( "SEARCH", needle );
        let viewAll = needle == "";
        wordTrs.forEach( w => trVisible( w.tr, viewAll || w.word.indexOf( needle ) >= 0 ) );
        sectionTrs.forEach( tr => trVisible( tr, viewAll ) );
        emptyTrs.forEach(   tr => trVisible( tr, viewAll ) );
    })
}

if( location.pathname == "/manual/Word-Index.html" )
    assimilateSearch();
else
    console.log( "Unhandles location", location.pathname );
