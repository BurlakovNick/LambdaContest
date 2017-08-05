let currentMoves = null;

$(() => {
    $("#moves").bind("input",
        (ev) => {
            const text = ev.currentTarget.value;
            currentMoves = getClaimsFromText(text);
            $("#moveNumber").val(currentMoves.length);
            playClaims(currentMoves, currentMoves.length);
        });

    $("#moveNumber").change((ev) => {
        const moveNumber = Number(ev.currentTarget.value);
        playClaims(currentMoves, moveNumber);
    });
});

function getClaimsFromText(text) {
    return text.split("\n")
        .map(x => /(\d+): (\d+) (\d+)->(\d+)/.exec(x))
        .map(x => ({
            punter: x[2],
            source: x[3],
            target: x[4]
        }));
}

function playClaims(claims, take) {
    for (let i = 0; i < claims.length; i++) {
        const claim = claims[i];
        const punter = i > take - 1
            ? null
            : claim.punter;
        updateEdgeOwner(punter, claim.source, claim.target);
    }
}

function updateEdgeOwner(punter, source, target) {
    const es = cy.edges("[source=\"" + source + "\"][target=\"" + target + "\"]");
    if (es.length > 0) {
        const e = es[0];
        e.data()["owner"] = punter;
        e.style("line-color", getPunterColour(punter));
    } else {
        throw new Error();
    }
}

const colours =
[
    "#1f77b4",
    "#aec7e8",
    "#ff7f0e",
    "#ffbb78",
    "#2ca02c",
    "#98df8a",
    "#d62728",
    "#ff9896",
    "#9467bd",
    "#c5b0d5",
    "#8c564b",
    "#c49c94",
    "#e377c2",
    "#f7b6d2",
    "#7f7f7f",
    "#c7c7c7",
    "#bcbd22",
    "#dbdb8d",
    "#17becf",
    "#9edae5"
];

function getPunterColour(punter) {
    return punter == null
        ? "#009"
        : colours[punter % colours.length];
}

//VIEWER

const json = {
    "maps": [
        {
            "filename": "/maps/sample.json",
            "name": "Sample from the Task Description",
            "num_nodes": 8,
            "num_edges": 12
        },
        { "filename": "/maps/lambda.json", "name": "Lambda", "num_nodes": 37, "num_edges": 60 },
        {
            "filename": "/maps/Sierpinski-triangle.json",
            "name": "Sierpinski triangle",
            "num_nodes": 42,
            "num_edges": 81
        },
        { "filename": "/maps/circle.json", "name": "Circle", "num_nodes": 27, "num_edges": 65 },
        { "filename": "/maps/randomMedium.json", "name": "Random1", "num_nodes": 97, "num_edges": 187 },
        { "filename": "/maps/randomSparse.json", "name": "Random2", "num_nodes": 86, "num_edges": 123 },
        { "filename": "/maps/tube.json", "name": "London Tube", "num_nodes": 301, "num_edges": 386 },
        {
            "filename": "/maps/oxford-center-sparse.json",
            "name": "Oxford City Centre",
            "num_nodes": 1425,
            "num_edges": 2020
        },
        { "filename": "/maps/oxford2-sparse-2.json", "name": "Oxford", "num_nodes": 2389, "num_edges": 3632 },
        { "filename": "/maps/edinburgh-sparse.json", "name": "Edinburgh", "num_nodes": 961, "num_edges": 1751 },
        { "filename": "/maps/boston-sparse.json", "name": "Boston", "num_nodes": 488, "num_edges": 945 },
        { "filename": "/maps/nara-sparse.json", "name": "Nara", "num_nodes": 1560, "num_edges": 2197 },
        { "filename": "/maps/van-city-sparse.json", "name": "Vancouver", "num_nodes": 1986, "num_edges": 3601 },
        { "filename": "/maps/gothenburg-sparse.json", "name": "Gothenburg", "num_nodes": 1175, "num_edges": 2234 }
    ],
    "other_maps": [
        { "filename": "/maps/nara-scaled.json", "name": "Nara", "num_nodes": 5374, "num_edges": 6086 },
        { "filename": "/maps/oxford-3000-nodes.json", "name": "Oxford (3)", "num_nodes": 3209, "num_edges": 4207 },
        { "filename": "/maps/oxford-sparse.json", "name": "Oxford (Sparse)", "num_nodes": 614, "num_edges": 1132 }
    ]
};

function loadMapList() {
    const select_elem = $("#maps-select");
    const maps = json.maps;

    for (let i = 0; i < maps.length; i++) {
        const map = maps[i];
        const opt = new Option(map.name + " (" + map.num_nodes + " sites and " + map.num_edges + " rivers )",
            map.filename);
        select_elem.append(opt);
    }

    select_elem.change(function(evt) {
        const item = select_elem.find(":selected");
        //alert("selected " + item.text() + ", val: " + item.val());
        selectMap(item.val());
    });

    selectMap(maps[0].filename);
}

function selectMap(url) {
    fetch(url, { mode: "no-cors" })
        .then(function(res) {
            return res.json();
        }).then(function(json) {
            if (cy.elements !== undefined) {
                cy.destroy();
            }
            initCy(json,
                function() {
                    cy.autolock(true);
                    bindCoreHandlers();
                    cy.edges().on("select", function(evt) { cy.edges().unselect() });
                });
        });

    $("#download-link").attr("href", url);
}

function bindCoreHandlers() {
    cy.edges().on("mouseover", function (evt) {
        this.style("content", this.data("owner"));
    });
    cy.edges().on("mouseout", function (evt) {
        this.style("content", "");
    });
}

$(function() {
    loadMapList();
});