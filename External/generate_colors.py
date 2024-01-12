from math import cos, degrees, sin
from colormath.color_objects import LabColor, HSLColor, AdobeRGBColor
from colormath.color_diff import delta_e_cie1994, delta_e_cie1976, delta_e_cie2000
from colormath.color_conversions import convert_color
from random import randint, random, randrange
from itertools import combinations
from tqdm import trange
from pathlib import Path
from json import dumps, dump, load
import numpy as np
import cv2
import click
import pandas as pd


L_MAX = 0.9
L_MIN = 0.3
KEEP_TOP_N = 100
ITERATIONS = 50000
N = 12
DIST_FUNCTION = delta_e_cie1976  # delta_e_cie2000


@click.group()
def cli():
    pass


@cli.command()
@click.option("-r", "random", is_flag=True)
def main(random):
    if random:
        selected_colors = random_select()
    else:
        selected_colors = uniform_sample()
    out_file_name = "results_{}.json"
    idx = 0
    while True:
        out_file_path = Path(out_file_name.format(idx))
        if not out_file_path.exists():
            break
        idx += 1

    with out_file_path.open("w") as f:
        dump(selected_colors, f)


def uniform_sample():
    selected_colors = []
    for s in range(0, 60, 10):
        colors = []
        for angle in range(s, s + 360, 120):
            for _a in [30, 75]:
                a = sin(degrees(angle)) * _a
                b = cos(degrees(angle)) * _a
                for l in range(40, 91, 40):
                    colors.append(LabColor(l, a, b))

        dist = []
        for c1, c2 in combinations(colors, 2):
             dist.append(DIST_FUNCTION(c1, c2))

        dist = min(dist)

        color_results = []
        for c in colors:
            c = convert_color(c, AdobeRGBColor)
            color_results.append([c.rgb_r, c.rgb_g, c.rgb_b])

        selected_colors.append({"score":dist, "colors": color_results})

    selected_colors.sort(key=lambda el: el["score"], reverse=True)
    display_top_results(selected_colors)
    return selected_colors


def random_select():
    selected_colors = []
    with trange(ITERATIONS) as t:
        for idx in t:
            colors = []
            for _ in range(N):
                while True:
                    c_rgb = AdobeRGBColor(random(), random(), random())
                    c_lab = convert_color(c_rgb, LabColor)
                    if c_lab.lab_l < L_MAX * 90 and c_lab.lab_l > L_MIN * 90:
                        colors.append(convert_color(c_rgb, LabColor))
                        break

            dist = []
            for c1, c2 in combinations(colors, 2):
                 dist.append(DIST_FUNCTION(c1, c2))

            dist = min(dist)

            color_results = []
            for c in colors:
                c = convert_color(c, AdobeRGBColor)
                color_results.append([c.rgb_r, c.rgb_g, c.rgb_b])

            selected_colors.append({"score":dist, "colors": color_results})

            selected_colors.sort(key=lambda el: el["score"], reverse=True)
            selected_colors = selected_colors[:KEEP_TOP_N]

            t.set_postfix({"max": "{:.1f}".format(selected_colors[0]['score'])})

            if idx % 10000 == 0:
                display_top_results(selected_colors, 5, 1)

        vals = np.array([c["score"] for c in selected_colors])
        print(f"Top 5: {vals[:5]}\nMin: {vals.min()}\nMax: {vals.max()}\nStd: {vals.std()}")

    return selected_colors


@cli.command()
@click.argument("result-file")
def display_image(result_file):
    with open(result_file) as f:
        result = load(f)

        display_top_results(result)


def display_top_results(result, n=20, wait=0):
    ims = []
    for _r in result[:n]:
        im = np.ndarray((20, 20*len(_r["colors"]),3), dtype=np.uint8)
        for i, (r, g, b) in enumerate(_r["colors"]):
            im[0:20, i*20:(i+1)*20] = AdobeRGBColor(r, g, b).get_upscaled_value_tuple()
        ims.append(im)
        ims.append(np.zeros((2, 20*len(_r["colors"]),3)).astype(np.uint8))

    ims = np.concatenate(ims)
    ims = cv2.cvtColor(ims, cv2.COLOR_RGB2BGR)
    cv2.imshow("", ims)
    cv2.waitKey(wait)


@cli.command()
@click.argument("result_file")
def write_image(result_file):
    with open(result_file) as f:
        result = load(f)
        for _r in result:
            im = np.ndarray((20, 20*len(_r["colors"]),3), dtype=np.uint8)
            for i, (r, g, b) in enumerate(_r["colors"]):
                im[0:20, i*20:(i+1)*20] = AdobeRGBColor(r, g, b).get_upscaled_value_tuple()
            im = cv2.cvtColor(im, cv2.COLOR_RGB2BGR)
            print("Score", _r["score"])
            cv2.imshow("", im)
            cv2.waitKey(1)

            done = False
            while True:
                _input = input("Write this out? [y/n]: ")
                if _input == "y":
                    done = True
                    break
                elif _input == "n":
                    break
                else:
                    print("huh?")

            if done:
                while True:
                    out_name = input("Out dir name: ")
                    out_dir = Path(__file__).parent.parent / "Assets" / "Sprites" / out_name
                    if out_dir.exists():
                        print(out_dir, "exists")
                        continue

                    out_dir.mkdir()

                    for i, (r, g, b) in enumerate(_r["colors"]):
                        im = np.ndarray((64,64,3), dtype=np.uint8)
                        im[:, :] = AdobeRGBColor(r, g, b).get_upscaled_value_tuple()
                        im = cv2.cvtColor(im, cv2.COLOR_RGB2BGR)

                        cv2.imwrite(str(out_dir / f"{i}.jpg"), im)
                    break
                break


@cli.command()
@click.argument("location")
@click.argument("ext")
def get_color_dist(location, ext):
    colors = []
    for img in Path(location).glob(f"*.{ext}"):
        frame = cv2.imread(str(img))
        b, g, r = frame[0, 0]
        print(img.name, f"{r/255:.2f}, {g/255:.2f}, {b/255:.2f}", "   Hex:",hex(r), hex(g), hex(b))
        colors.append(convert_color(AdobeRGBColor(r/255, g/255, b/255), LabColor))

    dist = []
    for c1, c2 in combinations(colors, 2):
         dist.append(DIST_FUNCTION(c1, c2))

    dist = min(dist)

    color_results = []
    for c in colors:
        c = convert_color(c, AdobeRGBColor)
        color_results.append([c.rgb_r, c.rgb_g, c.rgb_b])

    display_top_results([{"score": dist, "colors": color_results}])
    print("Score", dist)


@cli.command()
@click.argument("result_file")
@click.argument("idx", type=int)
def group_results_to_2(result_file, idx):
    with open(result_file) as f:
        result = load(f)
        _r = result[idx]
        c = []
        for i, (r, g, b) in enumerate(_r["colors"]):
            c.append(convert_color(AdobeRGBColor(r, g, b), LabColor))
        res = []
        c_idx = list(range(len(c)))
        for _c_idx in combinations(c_idx, round(len(c) / 2)):
            _c = [c[i] for i in _c_idx]
            within_vals = [DIST_FUNCTION(c1, c2) for (c1, c2) in combinations(_c, 2)]
            within_dist = sum(within_vals)
            other_idx = [i for i in c_idx if i not in _c_idx]
            other = [c[i] for i in other_idx]
            within_vals_other = [DIST_FUNCTION(c1, c2) for (c1, c2) in combinations(other, 2)]
            within_dist_other = sum(within_vals_other)
            between_vals = [DIST_FUNCTION(c1, c2) for c1 in _c for c2 in other]
            between_dist = sum(between_vals)
            res.append({
                "colors": list(_c),
                "other_colors": list(other),
                "within_dist": within_dist,
                "within_mean": np.mean(within_vals),
                "within_std": np.std(within_vals),
                "within_min": np.min(within_vals),
                "within_dist_other": within_dist_other,
                "within_mean_other": np.mean(within_vals_other),
                "within_std_other": np.std(within_vals_other),
                "within_min_other": np.min(within_vals_other),
                "between_dist": between_dist,
                "between_min": np.min(between_vals),
                "score": (np.min(within_vals) + np.min(within_vals_other) - np.min(between_vals))
            })

        res = pd.DataFrame(res)
        res = res.sort_values("score", ascending=False)

        print(res.iloc[:10].drop(["colors", "other_colors"], axis=1))
        display_res = [] 
        for idx, row in res.iloc[:5].iterrows():
            ca = [convert_color(_c, AdobeRGBColor) for _c in row.colors]
            ca = [(_c.rgb_r, _c.rgb_g, _c.rgb_b) for _c in ca]
            cb = [convert_color(_c, AdobeRGBColor) for _c in row.other_colors]
            cb = [(_c.rgb_r, _c.rgb_g, _c.rgb_b) for _c in cb]
            cc = [(50, 50, 50) for _ in cb]

            display_res.append({"colors": ca, "score":0})
            display_res.append({"colors": cb, "score":0})
            display_res.append({"colors": cc, "score":0})

        display_top_results(display_res, 30)


if __name__ == '__main__':
    cli()
