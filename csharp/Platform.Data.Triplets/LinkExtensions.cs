using System;
using System.Collections.Generic;
using System.Text;
using Platform.Data.Sequences;
using Platform.Data.Triplets.Sequences;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Triplets
{
    /// <summary>
    /// <para>
    /// Represents the link extensions.
    /// </para>
    /// <para></para>
    /// </summary>
    public static class LinkExtensions
    {
        /// <summary>
        /// <para>
        /// Sets the name using the specified link.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para></para>
        /// </param>
        /// <param name="name">
        /// <para>The name.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The link.</para>
        /// <para></para>
        /// </returns>
        public static Link SetName(this Link link, string name)
        {
            Link.Create(link, Net.Has, Link.Create(Net.Name, Net.ThatIsRepresentedBy, LinkConverter.FromString(name)));
            return link; // Chaining
        }

        /// <summary>
        /// <para>
        /// The link.
        /// </para>
        /// <para></para>
        /// </summary>
        private static readonly HashSet<Link> _linksWithNamesGatheringProcess = new HashSet<Link>();

        /// <summary>
        /// <para>
        /// Determines whether try get name.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para></para>
        /// </param>
        /// <param name="str">
        /// <para>The str.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The bool</para>
        /// <para></para>
        /// </returns>
        public static bool TryGetName(this Link link, out string str)
        {
            // Защита от зацикливания
            if (!_linksWithNamesGatheringProcess.Add(link))
            {
                str = "...";
                return true;
            }
            try
            {
                if (link != null)
                {
                    if (link.Linker == Net.And)
                    {
                        str = SequenceHelpers.FormatSequence(link);
                        return true;
                    }
                    else if (link.IsGroup())
                    {
                        str = LinkConverter.ToString(LinkConverter.ToList(link.Target));
                        return true;
                    }
                    else if (link.IsChar())
                    {
                        str = LinkConverter.ToChar(link).ToString();
                        return true;
                    }
                    else if (link.TryGetSpecificName(out str))
                    {
                        return true;
                    }

                    if (link.Source == link || link.Linker == link || link.Target == link)
                    {
                        return false;
                    }

                    if (link.Source.TryGetName(out string sourceName) && link.Linker.TryGetName(out string linkerName) && link.Target.TryGetName(out string targetName))
                    {
                        var sb = new StringBuilder();
                        sb.Append(sourceName).Append(' ').Append(linkerName).Append(' ').Append(targetName);
                        str = sb.ToString();
                        return true;
                    }
                }
                str = null;
                return false;
            }
            finally
            {
                _linksWithNamesGatheringProcess.Remove(link);
            }
        }

        /// <summary>
        /// <para>
        /// Determines whether try get specific name.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para></para>
        /// </param>
        /// <param name="name">
        /// <para>The name.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The bool</para>
        /// <para></para>
        /// </returns>
        public static bool TryGetSpecificName(this Link link, out string name)
        {
            string nameLocal = null;
            if (Net.Name.ReferersBySourceCount < link.ReferersBySourceCount)
            {
                Net.Name.WalkThroughReferersAsSource(referer =>
                {
                    if (referer.Linker == Net.ThatIsRepresentedBy)
                    {
                        if (Link.Exists(link, Net.Has, referer))
                        {
                            nameLocal = LinkConverter.ToString(referer.Target);
                            return false; // Останавливаем проход
                        }
                    }
                    return true;
                });
            }
            else
            {
                link.WalkThroughReferersAsSource(referer =>
                {
                    if (referer.Linker == Net.Has)
                    {
                        var nameLink = referer.Target;
                        if (nameLink.Source == Net.Name && nameLink.Linker == Net.ThatIsRepresentedBy)
                        {
                            nameLocal = LinkConverter.ToString(nameLink.Target);
                            return false; // Останавливаем проход
                        }
                    }
                    return true;
                });
            }

            name = nameLocal;
            return nameLocal != null;
        }

        // Проверка на пренадлежность классу
        /// <summary>
        /// <para>
        /// Determines whether is.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para></para>
        /// </param>
        /// <param name="@class">
        /// <para>The class.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The bool</para>
        /// <para></para>
        /// </returns>
        public static bool Is(this Link link, Link @class)
        {
            if (link.Linker == Net.IsA)
            {
                if (link.Target == @class)
                {
                    return true;
                }
                else
                {
                    return link.Target.Is(@class);
                }
            }
            return false;
        }

        // Несколько не правильное определение, так выйдет, что любая сумма входящая в диапазон значений char будет символом.
        // Нужно изменить определение чара, идеально: char consists of sum of [8, 64].
        /// <summary>
        /// <para>
        /// Determines whether is char.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The bool</para>
        /// <para></para>
        /// </returns>
        public static bool IsChar(this Link link) => CharacterHelpers.IsChar(link);

        /// <summary>
        /// <para>
        /// Determines whether is group.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The bool</para>
        /// <para></para>
        /// </returns>
        public static bool IsGroup(this Link link) => link != null && link.Source == Net.Group && link.Linker == Net.ThatConsistsOf;

        /// <summary>
        /// <para>
        /// Determines whether is sum.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The bool</para>
        /// <para></para>
        /// </returns>
        public static bool IsSum(this Link link) => link != null && link.Source == Net.Sum && link.Linker == Net.Of;

        /// <summary>
        /// <para>
        /// Determines whether is string.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The bool</para>
        /// <para></para>
        /// </returns>
        public static bool IsString(this Link link) => link != null && link.Source == Net.String && link.Linker == Net.ThatConsistsOf;

        /// <summary>
        /// <para>
        /// Determines whether is name.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The bool</para>
        /// <para></para>
        /// </returns>
        public static bool IsName(this Link link) => link != null && link.Source == Net.Name && link.Linker == Net.Of;

        /// <summary>
        /// <para>
        /// Gets the array of rererers by source using the specified link.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The link array</para>
        /// <para></para>
        /// </returns>
        public static Link[] GetArrayOfRererersBySource(this Link link)
        {
            if (link == null)
            {
                return new Link[0];
            }
            else
            {
                var array = new Link[link.ReferersBySourceCount];
                var index = 0;
                link.WalkThroughReferersAsSource(referer => array[index++] = referer);
                return array;
            }
        }

        /// <summary>
        /// <para>
        /// Gets the array of rererers by linker using the specified link.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The link array</para>
        /// <para></para>
        /// </returns>
        public static Link[] GetArrayOfRererersByLinker(this Link link)
        {
            if (link == null)
            {
                return new Link[0];
            }
            else
            {
                var array = new Link[link.ReferersByLinkerCount];
                var index = 0;
                link.WalkThroughReferersAsLinker(referer => array[index++] = referer);
                return array;
            }
        }

        /// <summary>
        /// <para>
        /// Gets the array of rererers by target using the specified link.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The link array</para>
        /// <para></para>
        /// </returns>
        public static Link[] GetArrayOfRererersByTarget(this Link link)
        {
            if (link == null)
            {
                return new Link[0];
            }
            else
            {
                var array = new Link[link.ReferersByTargetCount];
                var index = 0;
                link.WalkThroughReferersAsTarget(referer => array[index++] = referer);
                return array;
            }
        }

        /// <summary>
        /// <para>
        /// Walks the through sequence using the specified link.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">
        /// <para>The link.</para>
        /// <para></para>
        /// </param>
        /// <param name="action">
        /// <para>The action.</para>
        /// <para></para>
        /// </param>
        public static void WalkThroughSequence(this Link link, Action<Link> action) => SequenceWalker.WalkRight(link, x => x.Source, x => x.Target, x => x.Linker != Net.And, action);
    }
}
